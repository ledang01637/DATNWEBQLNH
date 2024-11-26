using DATN.Shared;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Text;
using System.Threading.Tasks;
using System;
using Microsoft.JSInterop;
using System.Net.Http.Json;
using Microsoft.AspNetCore.WebUtilities;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using Newtonsoft.Json.Linq;

namespace DATN.Client.Pages
{
    public partial class GGFBResponse
    {
        private Account account = new();
        private Customer customer = new(); 
        private bool isLoading = false;

        protected override async Task OnInitializedAsync()
        {
            var uri = new Uri(Navigation.Uri);
            var queryParams = QueryHelpers.ParseQuery(uri.Query);
            var token = queryParams.ContainsKey("token") ? queryParams["token"].ToString() : null;

            if (!string.IsNullOrEmpty(token))
            {
                try
                {
                    var handler = new JwtSecurityTokenHandler();

                    if (handler.ReadToken(token) is JwtSecurityToken jsonToken)
                    {
                        await _localStorageService.SetItemAsync("authToken", token);

                        var email = jsonToken.Claims.FirstOrDefault(c => c.Type == "Username")?.Value;

                        if(email == null) { await JS.InvokeVoidAsync("showAlert", "warning", "Thông báo", "Không tìm thấy Email check lại Server"); return; }

                        account = await httpClient.GetFromJsonAsync<Account>($"api/Account/GetAccountExist?Email={email}");

                        if (account == null) { await JS.InvokeVoidAsync("showAlert", "warning", "Thông báo", "Không tìm thấy tài khoản"); return; }

                        customer.Email = account.Email;

                    }
                    else
                    {
                        await JS.InvokeVoidAsync("showAlert", "error", "Lỗi", "Vui lòng đăng nhập lại");
                        return;
                    }
                }
                catch (Exception ex)
                {
                    await JS.InvokeVoidAsync("showAlert", "error", "Lỗi", "Vui lòng liên hệ Admin: " + ex.Message);
                    return;
                }
            }
            else
            {
                await JS.InvokeVoidAsync("showAlert", "error", "Lỗi", "Vui lòng đăng nhập lại");
                return;
            }
        }

        private async Task HandleValidSubmitAsync()
        {
            var exitsEmail = await httpClient.GetFromJsonAsync<Account>($"api/Account/GetAccountExist?Email={customer.Email}");

            if (exitsEmail != null && exitsEmail.AccountId > 0)
            {
                await JS.InvokeVoidAsync("showAlert", "error", "Thông báo", "Email đã tồn tại");
                return;
            }
            var exitsCustomerRes = await httpClient.PostAsJsonAsync("api/Customer/GetCustomerExist", customer);
            if (exitsCustomerRes.IsSuccessStatusCode)
            {
                var exitsCustomer = await exitsCustomerRes.Content.ReadFromJsonAsync<Customer>();
                if (exitsCustomer != null && exitsCustomer.CustomerId > 0)
                {
                    await JS.InvokeVoidAsync("showAlert", "error", "Thông báo", "Khách hàng đã tồn tại");
                    return;
                }
            }
            else
            {
                await JS.InvokeVoidAsync("showAlert", "error", "Thông báo", "Lỗi kiểm tra khách hàng");
                return;
            }
            isLoading = true;
            try
            {
                customer.AccountId = account.AccountId;

                var isAddCustomer = await AddNewCustomer(customer);

                if (!isAddCustomer) { await JS.InvokeVoidAsync("showAlert", "error", "Thông báo", "Lỗi thêm mới khách hàng"); return; }

                await JS.InvokeVoidAsync("showAlert", "success", "Thông báo", "Thêm thông tin thành công");
                Navigation.NavigateTo("/");
            }
            catch (Exception ex)
            {
                await JS.InvokeVoidAsync("showAlert", "error", "Thông báo", "Lỗi vui lòng liên hệ Admin: " + ex.Message);
                var query = $"[C#] fix error bằng tiếng Việt: {ex.Message}";
                await JS.InvokeVoidAsync("openChatGPT", query);

            }
            finally
            {
                isLoading = false;
                StateHasChanged();
            }
        }

        private async Task<bool> AddNewCustomer(Customer customer)
        {
            try
            {
                var response = await httpClient.PostAsJsonAsync("api/Customer/AddCustomer", customer);
                if (response.IsSuccessStatusCode)
                {
                    return true;
                }
                else
                {
                    var error = await response.Content.ReadAsStringAsync();
                    await JS.InvokeVoidAsync("showAlert", "error", "Thông báo", "Thêm khách hàng thất bại: " + error);
                    return false;
                }
            }
            catch (Exception ex)
            {
                await JS.InvokeVoidAsync("showAlert", "error", "Thông báo", "Vui lòng liên hệ admin: " + ex.Message);
                return false;
            }
        }
    }
}
