using DATN.Client.Service;
using DATN.Client.Shared;
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
using System.Net.Http;
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
        private DataCustomer dataCustomerModal = new();
        private readonly LoginRequest loginUser = new();
        private HubConnection hubConnection;

        [CascadingParameter]
        public EventCallback<string> notifyLayout {  get; set; }
        public string dataCustomerName { get; set; }


        private string ComboName;
        private int TotalQuantity;
        private decimal TotalAmount;
        private bool isProcessing = false;
        private string MessageText { get; set; }
        private string note;

        protected override async Task OnInitializedAsync()
        {
            isProcessing = true;
            hubConnection = new HubConnectionBuilder()
                .WithUrl(Navigation.ToAbsoluteUri("/ProcessHub"))
                .Build();
            await hubConnection.StartAsync();

            carts = await _cartService.GetCartAsync();
            await UpdateCartTotals();
            await LoadProductsAndCategories();
            await JS.InvokeVoidAsync("initializeIsotope");
            await LoadDataCustomer();
        }
        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            if (firstRender)
            {
                await JS.InvokeVoidAsync("initScrollToTop");
            }
        }


        private async Task LoadDataCustomer()
        {
            var localDataCustomer = await _localStorageService.GetAsync<DataCustomer>("dataCustomer");

            if (localDataCustomer == null)
            {
                await JS.InvokeVoidAsync("showModal", "CustomerDataModal");
                return;
            }

            try
            {
                var listData = await httpClient.GetFromJsonAsync<List<DataCustomer>>("api/DataCustomer/list");

                if (listData != null && listData.Count > 0)
                {
                    foreach (var d in listData)
                    {
                        if (d.CustomerGUID == localDataCustomer.CustomerGUID || (d.Name == localDataCustomer.Name && d.PhoneNumber == localDataCustomer.PhoneNumber))
                        {
                            //Truyền qua layout
                            dataCustomerName = d.Name;
                            _ = notifyLayout.InvokeAsync(dataCustomerName);
                            return;
                        }
                    }
                }
            }
            catch (FormatException)
            {
                await JS.InvokeVoidAsync("showAlert", "error", "Lỗi", "Dữ liệu khách hàng không hợp lệ.");
            }
 
        }

        private async Task OnSubmitForm()
        {
            dataCustomerModal.OrderIDs = new();

            var data = await VerifyAndSyncDataCustomer(dataCustomerModal.PhoneNumber);

            if(data != null)
            {
                await _localStorageService.SetAsync("dataCustomer", data);
                await JS.InvokeVoidAsync("closeModal", "CustomerDataModal");
                await JS.InvokeVoidAsync("showAlert", "success", "Thông báo", "Thành công");
                await LoadDataCustomer();
                return;
            }

            var localData = await _localStorageService.GetAsync<DataCustomer>("dataCustomer");

            if (localData != null)
            {
                var response = await httpClient.PostAsJsonAsync("api/DataCustomer/save", dataCustomerModal);

                var content = await response.Content.ReadAsStringAsync();

                if (!response.IsSuccessStatusCode)
                {
                    await JS.InvokeVoidAsync("showAlert", "error", "Lỗi", content);
                    return;
                }

                await _localStorageService.SetItemAsync("dataCustomer", content);
            }
            else
            {

                var listData = await httpClient.GetFromJsonAsync<List<DataCustomer>>("api/DataCustomer/list");

                if (listData != null && listData.Any(d => d.CustomerGUID == localData.CustomerGUID))
                {
                    await JS.InvokeVoidAsync("showAlert", "error", "Lỗi", "Khách hàng đã tồn tại.");
                    return;
                }

                var response = await httpClient.PostAsJsonAsync("api/DataCustomer/save", dataCustomerModal);
                var content = await response.Content.ReadAsStringAsync();

                if (!response.IsSuccessStatusCode)
                {
                    await JS.InvokeVoidAsync("showAlert", "error", "Lỗi", content);
                    return;
                }

                await _localStorageService.SetItemAsync("dataCustomer", content);
            }

            await JS.InvokeVoidAsync("closeModal", "CustomerDataModal");
            await JS.InvokeVoidAsync("showAlert", "success", "Thông báo", "Thành công");
            await LoadDataCustomer();
        }

        private async Task<DataCustomer> VerifyAndSyncDataCustomer(string phoneNumber)
        {
            try
            {
                var customerData = await httpClient.GetFromJsonAsync<DataCustomer>($"api/DataCustomer/getbyphone?phone={phoneNumber}");

                return customerData;
            }
            catch (HttpRequestException ex)
            {
                Console.WriteLine($"Lỗi khi kiểm tra khách hàng: {ex.Message}");
                return null;
            }
        }

        private async Task LoadProductsAndCategories()
        {
            try
            {
                var productTask = httpClient.GetFromJsonAsync<List<Product>>("api/Product/GetProductInclude");
                var categoryTask = httpClient.GetFromJsonAsync<List<Category>>("api/Category/GetCategories");

                await Task.WhenAll(productTask, categoryTask);

                products = (await productTask)?.Where(p => !p.IsDeleted).ToList() ?? new List<Product>();
                categories = (await categoryTask)?.Where(c => !c.IsDeleted).ToList() ?? new List<Category>();

                await LoadMenusAndAccounts();
            }
            catch (Exception ex)
            {
                var query = $"[C#] fix error bằng tiếng việt: {ex.Message}";
                await JS.InvokeVoidAsync("openChatGPT", query);
            }
            finally
            {
                isProcessing = false;
                StateHasChanged();
            }
        }

        private async Task LoadMenusAndAccounts()
        {
            var menuTask = httpClient.GetFromJsonAsync<List<Menu>>("api/Menu/GetMenu");
            var accountTask = httpClient.GetFromJsonAsync<List<Account>>("api/Account/GetAccount");

            await Task.WhenAll(menuTask, accountTask);

            menus = await menuTask ?? new List<Menu>();
            accounts = await accountTask ?? new List<Account>();

            var accountType = await CheckTypeAccount();

            if (accounts != null && accountType != "customer")
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
                    await JS.InvokeVoidAsync("showAlert", "error", "Lỗi", "Tài khoản mật khẩu không chính xác");
                    return;
                }
            }

            await JS.InvokeVoidAsync("initCallButton", "callButtonIndex", "expandButtons", "closeBtn");
            StateHasChanged();
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

            await LoadProductsAndCategories();

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
                await JS.InvokeVoidAsync("showAlert", "warning", "Thông báo", "Sản phẩm đã hết");
                ComboName = null;
            }

            isProcessing = false;
            StateHasChanged();
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
                Cart newCart = new()
                {
                    ProductId = product.ProductId,
                    ProductName = product.ProductName,
                    Price = product.Price,
                    ProductImage = product.ProductImage,
                    Quantity = 1,
                    UnitName = product.Units.UnitName,
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
            await JS.InvokeVoidAsync("closeModal", "ConfirmDeleteProductModal");
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
                await JS.InvokeVoidAsync("showAlert", "warning", "Vui lòng thêm món ăn");
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
                ListCartDTO.Note = note;
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
            if(string.IsNullOrEmpty(MessageText)) { await JS.InvokeVoidAsync("showAlert", "error", "Lỗi", "Vui lòng nhập yêu cầu"); return; }
            string token = await _localStorageService.GetItemAsync("n");
            if (string.IsNullOrEmpty(token))
            {
                await JS.InvokeVoidAsync("showAlert", "error", "Lỗi","Vui lòng quét QR");
                return;
            }

            int number = GetTableNumberFromToken(token);

            if (hubConnection is not null && hubConnection.State == HubConnectionState.Connected)
            {
                await hubConnection.SendAsync("SendMessageTable", MessageText, number);
                await JS.InvokeVoidAsync("closeModal", "sendMasageModal");
                await JS.InvokeVoidAsync("showAlert", "success", "Thành công","Đã gửi yêu cầu");
            }
            else
            {
                await JS.InvokeVoidAsync("showAlert", "error", "Lỗi", "Không thể kết nối tới server!");
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
        private async Task<string> CheckTypeAccount()
        {
            var token = await _localStorageService.GetItemAsync("authToken");
            if (!string.IsNullOrEmpty(token))
            {
                var handler = new JwtSecurityTokenHandler();

                if (handler.ReadToken(token) is JwtSecurityToken jwtToken)
                {
                    var accountTypeClaim = jwtToken.Claims.FirstOrDefault(c => c.Type.Equals("AccountType"));
                    return accountTypeClaim?.Value;
                }
                else
                {
                    await JS.InvokeVoidAsync("showAlert", "error", "Lỗi", "Không thể kiểm tra tài khoản");
                }
            }
            return null;
        }
    }
}
