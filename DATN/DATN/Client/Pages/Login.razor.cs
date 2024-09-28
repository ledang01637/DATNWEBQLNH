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



namespace DATN.Client.Pages
{
    
    public partial class Login
    {
        private LoginRequest loginUser = new LoginRequest();
        private bool loginFailed = false;
        private string currentPassword = string.Empty;
        private string confirmNewPassword = string.Empty;
        private string Token = "";

        private async Task HandleLogin()
        {
            var response = await httpClient.PostAsJsonAsync("api/AuthJWT/AuthUser", loginUser);
            if (response.IsSuccessStatusCode)
            {
                try
                {
                    var loginResponse = await response.Content.ReadFromJsonAsync<LoginRespone>();
                    if (loginResponse != null && loginResponse.SuccsessFull)
                    {
                        Token = loginResponse.Token;
                        var handler = new JwtSecurityTokenHandler();
                        var jsonToken = handler.ReadToken(Token) as JwtSecurityToken;
                        var Username = jsonToken.Claims.FirstOrDefault(c => c.Type == "Username")?.Value;
                        var isActive = jsonToken.Claims.FirstOrDefault(c => c.Type == "IsActive")?.Value;
                        var roleId = jsonToken.Claims.FirstOrDefault(c => c.Type == "RoleId")?.Value;
                        bool isActiveBool = false;

                        if (!string.IsNullOrEmpty(isActive))
                        {
                            bool.TryParse(isActive, out isActiveBool);
                        }

                        if (!isActiveBool)
                        {
                            await JS.InvokeVoidAsync("showAlert", "warning","Tài khoản bị khóa","Vui lòng liên hệ Admin");
                            await Task.Delay(1000);
                            Navigation.NavigateTo("/");
                            return;
                        }
                        var expiryTime = DateTime.Now.AddMinutes(30).ToString("o");
                        await _localStorageService.SetItemAsync("authToken", Token);
                        await _localStorageService.SetItemAsync("userName", Username);
                        await _localStorageService.SetItemAsync("expiryTime", expiryTime);
                        await JS.InvokeVoidAsync("showAlert", "success","Đăng nhập thành công","");
                        if (int.Parse(roleId) == 3)
                        {
                            Navigation.NavigateTo("/customer", true);
                        }
                        else
                        {
                            Navigation.NavigateTo("/manager", true);
                        }
                    }
                    else
                    {
                        await JS.InvokeVoidAsync("showAlert", "warning", "Tài khoản hoặc mật khẩu không đúng! ", "");
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
