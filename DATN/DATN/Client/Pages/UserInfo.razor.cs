using DATN.Shared;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.JSInterop;
using System;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Net.Http.Json;
using System.Threading.Tasks;

namespace DATN.Client.Pages
{
    public partial class UserInfo
    {
        private Customer customer = new();
        private string urlAccumulated = "/accumulatedpoints";
        protected override async Task OnInitializedAsync()
        {
            var accountType = await CheckTypeAccount();
            if(accountType != null && accountType != "customer") 
            {
                Navigation.NavigateTo("/login");
                return;
            }
            string accountId = await GetAccountIdAsync();
            if (string.IsNullOrEmpty(accountId))
            {
                await JS.InvokeVoidAsync("showAlert", "warning", "Thông báo", "Vui lòng đăng nhập lại");
                Navigation.NavigateTo("/login");
                return;
            }

            customer = await FetchCustomerByAccountAsync(int.Parse(accountId)) ?? new Customer();

            if (customer == null || customer.CustomerId <= 0)
            {
                await JS.InvokeVoidAsync("showAlert", "warning", "Thông báo", "Vui lòng đăng nhập lại và cập nhật thông tin");
                Navigation.NavigateTo("/login");
                return;
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
        private void Naviga(string url)
        {
            Navigation.NavigateTo(url);
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
        private void Logout()
        {
            Navigation.NavigateTo("/logout");
        }
    }
}
