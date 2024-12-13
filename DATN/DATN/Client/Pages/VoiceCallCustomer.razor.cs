using DATN.Client.Pages.AdminManager;
using DATN.Shared;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.JSInterop;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Net.Http.Json;
using System.Threading.Tasks;

namespace DATN.Client.Pages
{
    public partial class VoiceCallCustomer
    {
        public DotNetObjectReference<VoiceCallCustomer> dotNetObjectReference;

        private string token;
        private string from;
        private string to;

        protected override async Task OnInitializedAsync()
        {
           
            try
            {
                dotNetObjectReference = DotNetObjectReference.Create(this);

                token = await _localStorageService.GetItemAsync("n");
                to = await httpClient.GetStringAsync("api/Voice/get-message");

                if (token is null)
                {
                    await JS.InvokeVoidAsync("showAlert", "error", "Lỗi","Vui lòng quét QR");
                    Navigation.NavigateTo("/");
                    return;
                }
                from = GetTableNumberFromToken(token);
                if (from is null)
                {
                    await JS.InvokeVoidAsync("showAlert", "error", "Lỗi","Vui lòng quét QR");
                    Navigation.NavigateTo("/");
                    return;
                }
                if(to is null)
                {
                    await JS.InvokeVoidAsync("showAlert", "error", "Lỗi","Không tìm thấy người nhận");
                    Navigation.NavigateTo("/");
                    return;
                }
                await SetupCall(token, from.ToLower(), to.ToLower());
                await SetupVideo();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Không thể kết nối: {ex.Message}");
                await JS.InvokeVoidAsync("showAlert", "error", "Lỗi","Không thể kết nối tới server!");
                Navigation.NavigateTo("/");
                return;
            }
        }

        private async Task SetupCall(string token, string from, string to)
        {
            try
            {
                var response = await httpClient.PostAsJsonAsync("api/JwtTokenValidator/ValidateToken/", token);
                if(response.IsSuccessStatusCode)
                {
                    var handler = new JwtSecurityTokenHandler();

                    if (handler.ReadToken(token) is not JwtSecurityToken jsonToken)
                    {
                        await JS.InvokeVoidAsync("showAlert", "error", "Lỗi", "Vui lòng quét mã QR");
                    }
                    else
                    {
                        bool isCall = true;
                        await JS.InvokeVoidAsync("setupCall", token, from, to, isCall, dotNetObjectReference);
                        await JS.InvokeVoidAsync("layout");
                    }
                }
                else
                {
                    await JS.InvokeVoidAsync("showAlert", "error", "Lỗi", "Vui lòng đăng nhập lại");
                    Navigation.NavigateTo("/login");
                    return;
                }

                
            }
            catch
            {
                await JS.InvokeVoidAsync("showAlert", "error", "Lỗi","Vui lòng liên hệ Admin");
                Navigation.NavigateTo("/");
            }
        }
        private async Task SetupVideo()
        {
            await JS.InvokeVoidAsync("setupVideo", "btn-answer", "btn-call", "remoteVideo", "localVideo");
        }

        [JSInvokable("EndCall")]
        public void EndCall()
        {
            Navigation.NavigateTo("/");
        }

        [JSInvokable("BusyCall")]
        public async void BusyCall()
        {
            await JS.InvokeVoidAsync("showAlert","warning","Thông báo","Nhân viên đang có cuộc gọi khác");
            await Task.Delay(500);
        }

        [JSInvokable("LstCall")]

        public async Task LstCall(List<CallInfo> numberCall)
        {

        }
        private static string GetTableNumberFromToken(string token)
        {
            var handler = new JwtSecurityTokenHandler();
            var jwtToken = handler.ReadToken(token) as JwtSecurityToken;
            var userId = jwtToken?.Claims.FirstOrDefault(c => c.Type == "userId");
            return userId?.Value;
        }
    }
    public class CallInfo
    {
        public string FromNumber { get; set; }
        public string Time { get; set; }
    }
}
