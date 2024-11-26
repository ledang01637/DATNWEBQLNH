using DATN.Shared;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using System.IdentityModel.Tokens.Jwt;
using System.Net.Http;
using System.Threading.Tasks;
using System;
using Microsoft.JSInterop;
using System.Net.Http.Json;
using System.Linq;

namespace DATN.Client.Pages
{
    public partial class PersonalInformation
    {
        private Customer customer = new();
        private bool isLoading = false;
        private bool isEdit = false;
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
            try
            {
                isLoading = true;
                customer = await FetchCustomerByAccountAsync(int.Parse(accountId)) ?? new Customer();
            }
            catch(Exception ex)
            {
                await JS.InvokeVoidAsync("showAlert", "error", "Lỗi", "Vui long lien he Admin: " + ex.Message);
                return;
            }
            finally
            {
                isLoading = false;
                StateHasChanged();
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

        private void Edit()
        {
            isEdit = true;
        }

        private async Task HandleValidSubmitAsync()
        {
            var response = await httpClient.PutAsJsonAsync($"api/Customer/{customer.CustomerId}",customer);

            if(response.IsSuccessStatusCode)
                await JS.InvokeVoidAsync("showAlert", "success", "Thông báo", "Sửa thông tin thành công");
            else
                await JS.InvokeVoidAsync("showAlert", "error", "Thông báo", "Sửa thông tin thất bại");

            StateHasChanged();
        }
    }
}
