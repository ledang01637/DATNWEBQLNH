using Microsoft.JSInterop;
using System;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using DATN.Shared;
using static DATN.Client.Pages.Login;
using System.Collections.Generic;
using System.Net.Http.Json;
using System.Net.Mail;
using System.Net;
using System.Text.RegularExpressions;

namespace DATN.Client.Pages
{
    public partial class Register
    {
        private Account account = new();
        private Customer customer = new();
        private readonly int password = new Random().Next(10000, 99999);
        private string emailRegex = @"^[^\s@]+@[^\s@]+\.[^\s@]+$";
        private bool isLoading = false;
        private async Task HandleValidSubmitAsync()
        {
            isLoading = true;
            try
            {
                var exitsEmail = await httpClient.GetFromJsonAsync<Account>($"api/Account/GetAccountExist?Email={customer.Email}");

                if (exitsEmail != null && exitsEmail.AccountId > 0)
                {
                    await JS.InvokeVoidAsync("showAlert", "error","Thông báo", "Email đã tồn tại");
                    return;
                }
                var exitsCustomerRes = await httpClient.PostAsJsonAsync("api/Customer/GetCustomerExist", customer);
                if (exitsCustomerRes.IsSuccessStatusCode)
                {
                    var exitsCustomer = await exitsCustomerRes.Content.ReadFromJsonAsync<Customer>();
                    if(exitsCustomer != null && exitsCustomer.CustomerId > 0)
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
                using (SHA1 sha1 = SHA1.Create())
                {
                    byte[] passwordBytes = Encoding.UTF8.GetBytes(password.ToString());
                    byte[] hashedBytes = sha1.ComputeHash(passwordBytes);
                    account.Password = Convert.ToBase64String(hashedBytes);
                }
                account.CreateDate = DateTime.Now;
                account.UpdateDate = DateTime.Now;
                account.IsActive = true;
                account.IsDeleted = false;
                account.Email = customer.Email.ToLower();
                account.AccountType = "customer";
                var response = await httpClient.PostAsJsonAsync("api/Account/AddAccount", account);
                if (response.IsSuccessStatusCode)
                {
                    await SendEmailAsync();
                    var role = await httpClient.GetFromJsonAsync<int>("api/Role/GetRoleIdCustomer");
                    if(role == 0)
                    {
                        await JS.InvokeVoidAsync("showAlert", "warnint", "Thông báo", "Vui lòng thêm quyền customer");
                        return;
                    }
                    var creatRoleAccount = await response.Content.ReadFromJsonAsync<Account>();
                    if(creatRoleAccount  != null)
                    {
                        RoleAccount roleAccount = new()
                        {
                            IsDeleted = false,
                            AccountId = creatRoleAccount.AccountId,
                            RoleId = role
                        };
                        var responseRoleAccount = await httpClient.PostAsJsonAsync("api/RoleAccount/AddRoleAccount", roleAccount);

                        if (!responseRoleAccount.IsSuccessStatusCode)
                        {
                            await JS.InvokeVoidAsync("showAlert", "error", "Thông báo", "Lỗi tạo tài khoản");
                            return;
                        }
                        customer.AccountId = creatRoleAccount.AccountId;

                        var isAddCustomer = await AddNewCustomer(customer);

                        if(!isAddCustomer) { await JS.InvokeVoidAsync("showAlert", "error", "Thông báo", "Lỗi thêm mới khách hàng"); return; }

                        await JS.InvokeVoidAsync("showAlert", "success", "Đăng ký thành công", "Đã gửi mật khẩu mới về email của bạn");
                        Navigation.NavigateTo("/login");
                    }
                }
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

       

        private async Task SendEmailAsync()
        {
            string context = $"Ledang gửi quý khách,\n\n" +
                             $"\tBạn đang thực hiện tạo tài khoản mới.\n" +
                             $"\tĐây là mã mật khẩu mới của bạn: {password}\n" +
                             $"\tVui lòng đổi mật khẩu sau khi đăng ký thành công!\n\n" +
                             $"Cảm ơn.";

            if (string.IsNullOrWhiteSpace(customer.Email))
            {
                await JS.InvokeVoidAsync("showAlert", "error", "Thông báo", "Email không được để trống");
                return;
            }

            if (!Regex.IsMatch(customer.Email, emailRegex))
            {
                await JS.InvokeVoidAsync("showAlert", "error", "Thông báo", "Email không hợp lệ");
                return;
            }

            try
            {
                EmailRequest request = new();
                request.To = customer.Email;
                request.Subject = "OTP nhận mật khẩu mới";
                request.Body = context;

                var response = await httpClient.PostAsJsonAsync("api/Email/send-email", request);
                if(!response.IsSuccessStatusCode)
                {
                    await JS.InvokeVoidAsync("showAlert", "error", "Thông báo", "Lỗi không thể gửi mật khẩu mới");
                    return;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Lỗi gửi email: {ex.Message}");
                await JS.InvokeVoidAsync("showAlert", "error", "Thông báo", "Gửi mật khẩu thất bại");
            }
        }

    }
}
