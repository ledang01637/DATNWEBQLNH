using Microsoft.JSInterop;
using System;
using System.Collections.Generic;
using System.Net.Http.Json;
using System.Threading.Tasks;
using DATN.Shared;
using System.Net.Http;
using System.Text.Json;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using Microsoft.AspNetCore.Components.Authorization;
using DATN.Client.Shared;
using Microsoft.AspNetCore.Components;



namespace DATN.Client.Pages
{

    public partial class Login
    {
        private LoginRequest loginUser = new LoginRequest();
        private bool loginFailed = false;
        private string currentPassword = string.Empty;
        private string confirmNewPassword = string.Empty;
        private string Token = "";
        private Type CurrentLayout { get; set; }

        private async Task HandleLogin()
        {
            if (string.IsNullOrEmpty(loginUser.Email) || string.IsNullOrEmpty(loginUser.Password))
            {
                await JS.InvokeVoidAsync("showAlert", "error", "Vui nhập tài khoản và mật khẩu");
                return;
            }

            var response = await httpClient.PostAsJsonAsync("api/AuthJWT/AuthUser", loginUser);
            if (response.IsSuccessStatusCode)
            {
                try
                {
                    var loginResponse = await response.Content.ReadFromJsonAsync<LoginRespone>();
                    if (loginResponse?.SuccsessFull == true)
                    {
                        Token = loginResponse.Token;
                        var handler = new JwtSecurityTokenHandler();
                        var jsonToken = handler.ReadToken(Token) as JwtSecurityToken;

                        var Username = jsonToken?.Claims.FirstOrDefault(c => c.Type == "Username")?.Value;
                        var isActive = jsonToken?.Claims.FirstOrDefault(c => c.Type == "IsActive")?.Value;
                        var roleId = jsonToken?.Claims.FirstOrDefault(c => c.Type == "RoleId")?.Value;
                        var accountId = jsonToken?.Claims.FirstOrDefault(c => c.Type == "AccountId")?.Value;

                        if (bool.TryParse(isActive, out bool isActiveBool) && !isActiveBool)
                        {
                            await JS.InvokeVoidAsync("showAlert", "warning", "Tài khoản bị khóa", "Vui lòng liên hệ Admin");
                            return;
                        }

                        await _localStorageService.SetItemAsync("authToken", Token);
                        var expiryTime = DateTime.Now.AddMinutes(45).ToString("o");
                        var authState = await authenticationStateProvider.GetAuthenticationStateAsync();
                        var user = authState.User;

                        if (user.Identity.IsAuthenticated && (user.IsInRole("1") || user.IsInRole("2") || user.IsInRole("3")))
                        {
                            var res = await httpClient.PostAsJsonAsync("api/AuthJWT/ManagerToken/", loginUser);
                            if (res.IsSuccessStatusCode)
                            {
                                var resResponese = await res.Content.ReadFromJsonAsync<QRResponse>();
                                if (resResponese?.IsSuccessFull == true)
                                {
                                    await _localStorageService.SetItemAsync("m", resResponese.Token);
                                    await _localStorageService.SetItemAsync("userName", Username);
                                    await _localStorageService.SetItemAsync("expiryTime", expiryTime);
                                    await _localStorageService.SetItemAsync("AccountId", accountId);
                                }
                            }
                            CurrentLayout = typeof(LayoutAdmin);
                            Navigation.NavigateTo("/admin", true);
                        }
                        else
                        {
                            CurrentLayout = typeof(MainLayout);
                            await _localStorageService.SetItemAsync("userName", Username);
                            await _localStorageService.SetItemAsync("expiryTime", expiryTime);
                            await _localStorageService.SetItemAsync("AccountId", accountId);
                            Navigation.NavigateTo("/", true);
                        }
                    }
                    else
                    {
                        await JS.InvokeVoidAsync("showAlert", "warning", "Tài khoản hoặc mật khẩu không đúng!", "");
                    }
                }
                catch (JsonException ex)
                {
                    var query = $"[C#] fix error: {ex.Message}";
                    await JS.InvokeVoidAsync("openChatGPT", query);
                    Token = $"JSON parse error: {ex.Message}";
                }
            }
            else
            {
                Token = "Server error or invalid request.";
            }
        }

        private async Task HandlePasswordChange()
        {
            //if (loggedInUser == null)
            //{
            //    // Nếu người dùng chưa đăng nhập
            //    await JS.InvokeVoidAsync("showAlert", "Chưa đăng nhập");
            //    return;
            //}
            //if (passwordChangeModel.NewPassword != passwordChangeModel.ConfirmNewPassword)
            //{
            //    // Kiểm tra nếu mật khẩu mới không khớp với xác nhận mật khẩu
            //    await JS.InvokeVoidAsync("showAlert", "Mật khẩu xác nhận không khớp");
            //    return;
            //}
            //// Cập nhật mật khẩu mới
            //loggedInUser.Password = passwordChangeModel.NewPassword;
            //await JS.InvokeVoidAsync("showAlert", "Đổi mật khẩu thành công");
        }
    }

}
