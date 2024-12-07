using DATN.Client.Shared;
using DATN.Shared;
using Microsoft.JSInterop;
using System.IdentityModel.Tokens.Jwt;
using System.Net.Http.Json;
using System.Threading.Tasks;
using System;
using System.Text.Json;
using System.Linq;

namespace DATN.Client.Pages.AdminManager
{
    public partial class LoginAdmin
    {
        private LoginRequest loginUser = new LoginRequest();
        private bool loginFailed = false;
        private string Token = "";
        private bool IsProcess = false;

        private async Task HandleLogin()
        {
            IsProcess = true;
            try
            {
                if (string.IsNullOrEmpty(loginUser.Email) || string.IsNullOrEmpty(loginUser.Password))
                {
                    await JS.InvokeVoidAsync("showAlert", "error", "Vui nhập tài khoản và mật khẩu");
                    return;
                }

                var response = await httpClient.PostAsJsonAsync("api/AuthJWT/AuthUser", loginUser);
                if (response.IsSuccessStatusCode)
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

                        if (user.Identity.IsAuthenticated && (user.IsInRole("admin")) || user.Identity.IsAuthenticated && (user.IsInRole("employee")) || user.Identity.IsAuthenticated && (user.IsInRole("chef")))
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
                            await JS.InvokeVoidAsync("showAlert", "success", "Thành công");
                            if (user.Identity.IsAuthenticated && (user.IsInRole("admin")))
                            {
                                Navigation.NavigateTo("/Statistic", true);
                            }
                            else if(user.Identity.IsAuthenticated && (user.IsInRole("chef")))
                            {
                                Navigation.NavigateTo("/manager-chef", true);
                            }
                            else
                            {
                                Navigation.NavigateTo("/manager", true);
                            }
                        }
                        else
                        {
                            await JS.InvokeVoidAsync("showAlert", "warning", "Cảnh báo", "Tài khoản không có quyền truy cập");
                        }
                    }
                    else
                    {
                        await JS.InvokeVoidAsync("showAlert", "warning", "Cảnh báo", "Tài khoản hoặc mật khẩu không đúng");
                    }
                }
                else
                {
                    Token = "Server error or invalid request.";
                }
            }
            catch (JsonException ex)
            {
                var query = $"[C#] fix error: {ex.Message}";
                await JS.InvokeVoidAsync("openChatGPT", query);
                Token = $"JSON parse error: {ex.Message}";
            }
            finally
            {
                IsProcess = false;
            }
            
        }
    }
}
