using DATN.Client.Service;
using DATN.Shared;
using Microsoft.AspNetCore.Components;
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
        private List<Product> products = new List<Product>();
        private List<Menu> menus = new List<Menu>();
        private List<Category> categories = new List<Category>();
        private List<MenuItem> menuItems = new List<MenuItem>();
        private List<Account> accounts = new List<Account>();
        private List<Cart> carts = new List<Cart>();
        private LoginRequest loginUser = new LoginRequest();
        private string ComboName;
        private bool isGridView = true;
        private int TotalQuantity;
        private decimal TotalAmount;
        private bool isProcessing = false;

        protected override async Task OnInitializedAsync()
        {
            carts = await _cartService.GetCartAsync(); 
            await UpdateCartTotals();
            await LoadAll();
            await JS.InvokeVoidAsync("initializeIsotope");
        }
        private async Task LoadAll()
        {
            try
            {
                products = await httpClient.GetFromJsonAsync<List<Product>>("api/Product/GetProduct");
                if (products != null)
                {
                    products = products.Where(a => a.IsDelete == false).ToList();
                }
                categories = await httpClient.GetFromJsonAsync<List<Category>>("api/Category/GetCategories");
                if (categories != null)
                {
                    categories = categories.Where(a => a.IsDelete == false).ToList();
                }
                menus = await httpClient.GetFromJsonAsync<List<Menu>>("api/Menu/GetMenu");
                accounts = await httpClient.GetFromJsonAsync<List<Account>>("api/Account/GetAccount");
                if(accounts != null)
                {
                    loginUser.Username = "no account";
                    loginUser.Password = "123456";
                    var response = await httpClient.PostAsJsonAsync("api/AuthJWT/AuthUser", loginUser);
                    if (response.IsSuccessStatusCode)
                    {
                        var loginResponse = await response.Content.ReadFromJsonAsync<LoginRespone>();
                        if (loginResponse != null && loginResponse.SuccsessFull)
                        {
                            var handler = new JwtSecurityTokenHandler();
                            var jsonToken = handler.ReadToken(loginResponse.Token) as JwtSecurityToken;
                            var accountId = jsonToken.Claims.FirstOrDefault(c => c.Type == "AccountId")?.Value;

                            var expiryTime = DateTime.Now.AddMinutes(45).ToString("o");
                            await _localStorageService.SetItemAsync("expiryTime", expiryTime);
                            await _localStorageService.SetItemAsync("AccountId", accountId);
                            await _localStorageService.SetItemAsync("authToken", loginResponse.Token);
                        }
                    }
                    else
                    {
                        await JS.InvokeVoidAsync("showLog", "Tài khoản mật khẩu không chính xác");
                    }
                }
                await JS.InvokeVoidAsync("initDrag", "callButtonIndex", "expandButtons", "callStaffBtn", "closeBtn");
                StateHasChanged();

            }
            catch (Exception ex)
            {
                var query = $"[C#] fix error bằng tiếng việt: {ex.Message}";
                await JS.InvokeVoidAsync("openChatGPT", query);
            }
        }

        private async Task AddToCart(Product product)
        {
            if (carts != null)
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
                        Quantity = 1
                    };
                    carts.Add(newCart);
                }

                TotalQuantity += 1;
                TotalAmount += product.Price;

                StateHasChanged();

                _ = Task.Run(async () =>
                {
                    await _cartService.SaveCartAsync(carts);
                });
            }
            else
            {
                await JS.InvokeVoidAsync("showLog", "Cart is null");
            }
        }



        private async Task UpdateCartTotals()
        {
            TotalQuantity = 0;
            TotalAmount = 0;

            carts = await _cartService.GetCartAsync();
            if(carts != null && carts.Count() > 0)
            {
                foreach (var c in carts)
                {
                    TotalQuantity += c.Quantity;
                    TotalAmount += (c.Price * c.Quantity);
                }
            }
            await Task.Delay(1000);
            StateHasChanged();

        }

        private async Task IncreaseQuantity(int ProId)
        {

            var cartItem = carts.FirstOrDefault(c => c.ProductId == ProId);
            if (cartItem != null)
            {
                cartItem.Quantity += 1;
                TotalQuantity += 1;
                TotalAmount += cartItem.Price;
                await _cartService.SaveCartAsync(carts);
                StateHasChanged();
            }
            else
            {
                await JS.InvokeVoidAsync("showLog", "Cart is null");
            }
            await Task.Delay(1000);
            isProcessing = false;
            StateHasChanged();
        }

        private async Task DecreaseQuantity(int ProId)
        {
            isProcessing = true;

            var cartItem = carts.FirstOrDefault(c => c.ProductId == ProId);

            if (cartItem != null && cartItem.Quantity > 0)
            {
                cartItem.Quantity -= 1;
                TotalQuantity -= 1;
                TotalAmount -= cartItem.Price;

                if (cartItem.Quantity == 0)
                {
                    carts.Remove(cartItem);
                }

                await _cartService.SaveCartAsync(carts);

                StateHasChanged();
            }
            else
            {
                await JS.InvokeVoidAsync("showLog", "Cart is null or quantity <= 0");
            }
            StateHasChanged();
        }



        private async Task LoadCombo(int MenuId)
        {
            isProcessing = true;
            if (MenuId > 0)
            {
                var selectedMenu = menus.FirstOrDefault(a => a.MenuId == MenuId);
                ComboName = selectedMenu?.MenuName;

                menuItems = await httpClient.GetFromJsonAsync<List<MenuItem>>("api/MenuItem/GetMenuItem");

                menuItems = menuItems.Where(item => item.MenuId == MenuId).ToList();

                var productIds = menuItems.Select(item => item.ProductId).ToHashSet();

                await LoadAll();

                var prods = products.Where(p => productIds.Contains(p.ProductId)).ToList();

                if(prods.Count > 0)
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
            }
            isProcessing = false;
        }

        private async void NaviOrderList()
        {
            if(!(carts.Count > 0)) 
            {
                await JS.InvokeVoidAsync("showAlert", "warning", "Giỏ hàng rỗng", "Vui lòng chọn món ăn");
                return;
            }
            await JS.InvokeVoidAsync("closeModal", "cartModal");
            Navigation.NavigateTo("/order-list");
        }
        private async void NaviCustomer()
        {
            var token = await _localStorageService.GetItemAsync("n");
            Navigation.NavigateTo("/customer");

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
