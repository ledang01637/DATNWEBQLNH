using DATN.Client.Service;
using DATN.Shared;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.EntityFrameworkCore;
using Microsoft.JSInterop;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Net.Http.Json;
using System.Threading.Tasks;

namespace DATN.Client.Pages
{
    public partial class Index
    {
        private List<Product> products = new();
        private List<Menu> menus = new();
        private List<Category> categories = new();
        private List<MenuItem> menuItems = new();
        private List<Account> accounts = new();
        private List<Cart> carts = new();
        private readonly LoginRequest loginUser = new();
        private HubConnection hubConnection;
        private string ComboName;
        private bool isGridView = true;
        private int TotalQuantity;
        private decimal TotalAmount;
        private bool isProcessing = false;
        private string messageText { get; set; }
        private string note;

        protected override async Task OnInitializedAsync()
        {
            hubConnection = new HubConnectionBuilder()
                .WithUrl(Navigation.ToAbsoluteUri("/ProcessHub"))
                .Build();
            await hubConnection.StartAsync();

            carts = await _cartService.GetCartAsync();
            await UpdateCartTotals();
            await LoadAll();
            await JS.InvokeVoidAsync("initializeIsotope");
        }
        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            if (firstRender)
            {
                await JS.InvokeVoidAsync("initScrollToTop");
            }
        }
        private async Task LoadAll()
        {
            try
            {
                var productTask = httpClient.GetFromJsonAsync<List<Product>>("api/Product/GetProduct");
                var categoryTask = httpClient.GetFromJsonAsync<List<Category>>("api/Category/GetCategories");
                var menuTask = httpClient.GetFromJsonAsync<List<Menu>>("api/Menu/GetMenu");
                var accountTask = httpClient.GetFromJsonAsync<List<Account>>("api/Account/GetAccount");

                await Task.WhenAll(productTask, categoryTask, menuTask, accountTask);

                products = (await productTask)?.Where(p => !p.IsDeleted).ToList() ?? new List<Product>();
                categories = (await categoryTask)?.Where(c => !c.IsDeleted).ToList() ?? new List<Category>();
                menus = await menuTask ?? new List<Menu>();
                accounts = await accountTask ?? new List<Account>();

                if (accounts != null)
                {
                    loginUser.Email = "no account";
                    loginUser.Password = "123456";

                    var response = await httpClient.PostAsJsonAsync("api/AuthJWT/AuthUser", loginUser);
                    if (response.IsSuccessStatusCode)
                    {
                        var loginResponse = await response.Content.ReadFromJsonAsync<LoginRespone>();
                        if (loginResponse?.SuccsessFull == true)
                        {
                            var handler = new JwtSecurityTokenHandler();
                            var jsonToken = handler.ReadToken(loginResponse.Token) as JwtSecurityToken;
                            var accountId = jsonToken?.Claims.FirstOrDefault(c => c.Type == "AccountId")?.Value;

                            var expiryTime = DateTime.Now.AddMinutes(45).ToString("o");
                            await Task.WhenAll(
                                _localStorageService.SetItemAsync("expiryTime", expiryTime),
                                _localStorageService.SetItemAsync("AccountId", accountId),
                                _localStorageService.SetItemAsync("authToken", loginResponse.Token)
                            );
                        }
                    }
                    else
                    {
                        await JS.InvokeVoidAsync("showLog", "Tài khoản mật khẩu không chính xác");
                    }
                }

                await JS.InvokeVoidAsync("initCallButton", "callButtonIndex", "expandButtons", "closeBtn");
                StateHasChanged();
            }
            catch (Exception ex)
            {
                var query = $"[C#] fix error bằng tiếng việt: {ex.Message}";
                await JS.InvokeVoidAsync("openChatGPT", query);
            }
        }

        private async Task LoadCombo(int MenuId)
        {
            if (MenuId <= 0) return;

            isProcessing = true;

            var selectedMenu = menus.FirstOrDefault(a => a.MenuId == MenuId);
            ComboName = selectedMenu?.MenuName ?? "No Combo";

            menuItems = (await httpClient.GetFromJsonAsync<List<MenuItem>>("api/MenuItem/GetMenuItem"))
                                        .Where(item => item.MenuId == MenuId)
                                        .ToList();

            var productIds = menuItems.Select(item => item.ProductId).ToHashSet();

            await LoadAll();

            var prods = products.Where(p => productIds.Contains(p.ProductId)).ToList();
            if (prods.Any())
            {
                foreach (var product in prods)
                {
                    await AddToCart(product);
                }
            }
            else
            {
                await JS.InvokeVoidAsync("showAlert", "warning", "Sản phẩm đã hết");
            }

            StateHasChanged();
            isProcessing = false;
        }

        private async Task AddToCart(Product product)
        {
            var existingCart = carts.FirstOrDefault(c => c.ProductId == product.ProductId);

            if (existingCart != null)
            {
                existingCart.Quantity += 1;
            }
            else
            {
                Cart newCart = new Cart
                {
                    ProductId = product.ProductId,
                    ProductName = product.ProductName,
                    Price = product.Price,
                    ProductImage = product.ProductImage,
                    Quantity = 1,
                };
                carts.Add(newCart);
            }
            _ = _cartService.SaveCartAsync(carts);
            await UpdateCartTotals(product.Price, 1);
        }
        private async Task RemoveFromCart(Cart product)
        {
            var existingCart = carts.FirstOrDefault(c => c.ProductId == product.ProductId);

            if (existingCart != null)
            {
                carts.Remove(existingCart);
                _ = _cartService.SaveCartAsync(carts);
                await UpdateCartTotals(-product.Price * product.Quantity, -product.Quantity);
            }
        }

        private async Task RemoveAllCarts()
        {
            carts.Clear();
            TotalQuantity = 0;
            TotalAmount = 0;

            await _cartService.SaveCartAsync(carts);
            StateHasChanged();
        }
        private Task UpdateCartTotals()
        {
            TotalQuantity = carts.Sum(c => c.Quantity);
            TotalAmount = carts.Sum(c => c.Price * c.Quantity);
            StateHasChanged();
            return Task.CompletedTask;
        }

        private Task UpdateCartTotals(decimal priceChange, int quantityChange)
        {


            TotalQuantity += quantityChange;

            if(priceChange < 0) 
            {
                TotalAmount += priceChange;
                StateHasChanged();
                return Task.CompletedTask;
            }

            TotalAmount += priceChange * quantityChange;

            StateHasChanged();
            return Task.CompletedTask;
        }

        private async Task ModifyQuantity(int productId, int change)
        {
            var cartItem = carts.FirstOrDefault(c => c.ProductId == productId);
            if (cartItem == null)
            {
                await JS.InvokeVoidAsync("showAlert", "warning", "Cart item not found");
                return;
            }

            cartItem.Quantity += change;

            if (cartItem.Quantity <= 0)
            {
                carts.Remove(cartItem);
            }

            await UpdateCartTotals(cartItem.Price, change);
            await _cartService.SaveCartAsync(carts);
        }

        private async Task IncreaseQuantity(int productId)
        {
            await ModifyQuantity(productId, 1);
        }

        private async Task DecreaseQuantity(int productId)
        {
            await ModifyQuantity(productId, -1);
        }
        private async void NaviOrderList()
        {
            if (!(carts.Count > 0))
            {
                await JS.InvokeVoidAsync("showAlert", "warning", "Giỏ hàng rỗng", "Vui lòng chọn món ăn");
                return;
            }
            await JS.InvokeVoidAsync("closeModal", "cartModal");
            if(!string.IsNullOrEmpty(note)) 
            {
                ListCart.Note = note;
            }
            Navigation.NavigateTo("/order-list");
        }
        private async void NaviCustomer()
        {
            await _localStorageService.GetItemAsync("n");
            Navigation.NavigateTo("/customer");

        }

        private async Task SendMessage()
        {
            if(string.IsNullOrEmpty(messageText)) { await JS.InvokeVoidAsync("showAlert", "warning", "Lỗi", "Vui lòng nhập yêu cầu"); return; }
            string token = await _localStorageService.GetItemAsync("n");
            if (string.IsNullOrEmpty(token))
            {
                await JS.InvokeVoidAsync("showAlert", "error", "Token is null");
                return;
            }

            int number = GetTableNumberFromToken(token);

            if (hubConnection is not null && hubConnection.State == HubConnectionState.Connected)
            {
                await hubConnection.SendAsync("SendMessageTable", messageText, number.ToString());
                await JS.InvokeVoidAsync("closeModal", "sendMasageModal");
                await JS.InvokeVoidAsync("showAlert", "success", "Thành công","Đã gửi yêu cầu");
            }
            else
            {
                await JS.InvokeVoidAsync("showAlert", "error", "Không thể kết nối tới server!");
                return;
            }
        }

        private async Task ScrollToTop()
        {
            await JS.InvokeVoidAsync("scrollToTop");
        }

        private static int GetTableNumberFromToken(string token)
        {
            var handler = new JwtSecurityTokenHandler();
            var jwtToken = handler.ReadToken(token) as JwtSecurityToken;
            var userId = jwtToken?.Claims.FirstOrDefault(c => c.Type == "userId");
            return int.Parse(userId?.Value);
        }

        //private string GetGridColumnClass()
        //{
        //    return isGridView ? "col-sm-6 col-lg-4" : "col-lg-4";
        //}
        //private void ToggleGridView()
        //{
        //    isGridView = !isGridView;
        //}
    }
}
