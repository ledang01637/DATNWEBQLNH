using DATN.Shared;
using Microsoft.JSInterop;
using System.IdentityModel.Tokens.Jwt;
using System.Net.Http.Json;
using System;
using System.Threading.Tasks;
using System.Linq;
using System.Collections.Generic;

namespace DATN.Client.Pages
{
    public partial class PurchaseHistory
    {
        private Customer customer = new();
        private List<Order> orders = new();
        private List<Order> _allOrders = new();

        protected override async Task OnInitializedAsync()
        {
            var accountType = await CheckTypeAccount();
            if (accountType != null && accountType != "customer")
            {
                Navigation.NavigateTo("/login");
                return;
            }
            string accountId = await GetAccountIdAsync();
            if (string.IsNullOrEmpty(accountId))
            {
                await JS.InvokeVoidAsync("showAlert", "warning", "Thông báo", "Vui lòng đăng nhập lại");
                return;
            }

            customer = await FetchCustomerByAccountAsync(int.Parse(accountId)) ?? new Customer();
            if(customer != null && customer.CustomerId > 0)
            {
                _allOrders = await httpClient.GetFromJsonAsync<List<Order>>($"api/Order/GetOrderLstByCustomer?customerId={customer.CustomerId}");
                orders = _allOrders;
            }
            
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
                    await JS.InvokeVoidAsync("showAlert", "error", "Lỗi", "Vui lòng liên hệ Admin");
                }
            }
            return null;
        }
        private async Task<string> GetAccountIdAsync()
        {
            var token = await _localStorageService.GetItemAsync("authToken");
            if (string.IsNullOrEmpty(token)) return null;

            var handler = new JwtSecurityTokenHandler();
            var jwtToken = handler.ReadToken(token) as JwtSecurityToken;
            return jwtToken?.Claims.FirstOrDefault(c => c.Type == "AccountId")?.Value;
        }
        private async Task<Customer> FetchCustomerByAccountAsync(int accountId)
        {
            try
            {
                var response = await httpClient.PostAsJsonAsync("api/Customer/GetCustomerByAccountId", accountId);
                return response.IsSuccessStatusCode ? await response.Content.ReadFromJsonAsync<Customer>() : null;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error fetching customer: {ex.Message}");
                return null;
            }
        }
        private void Search(string text)
        {
            if (string.IsNullOrEmpty(text))
            {
                orders = _allOrders; 
            }
            else
            {
                orders = _allOrders
                 .Where(o => o.OrderId.ToString().Contains(text, StringComparison.OrdinalIgnoreCase) ||
                             (o.Note != null && o.Note.Contains(text, StringComparison.OrdinalIgnoreCase)) ||
                             o.OrderItems.Any(oi => oi.Products.ProductName.Contains(text, StringComparison.OrdinalIgnoreCase)))
                 .ToList();

            }
        }
    }
}
