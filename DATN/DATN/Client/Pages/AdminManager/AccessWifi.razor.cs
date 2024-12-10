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
    public partial class AccessWifi
    {
        private string IP;
        private async Task HandleSubmit()
        {
            if (string.IsNullOrEmpty(IP) && !IsValidIp(IP))
            {
                await JS.InvokeVoidAsync("showAlert", "error", "Lỗi", "IP không hợp lệ");
                return;
            }

            var response = await httpClient.PostAsJsonAsync("api/Network/post-wifi-ip", IP);

            var result = await response.Content.ReadAsStringAsync();

            if (response.IsSuccessStatusCode)
            {
                await JS.InvokeVoidAsync("showAlert", "success", "Thông báo", result);
                return;
            }
            else
            {
                if (response.StatusCode == System.Net.HttpStatusCode.BadRequest)
                {
                    await JS.InvokeVoidAsync("showAlert", "error", "Lỗi", result);
                }
                else
                {
                    await JS.InvokeVoidAsync("showAlert", "error", "Lỗi", result);
                }
            }
        }
        private static bool IsValidIp(string ip)
        {
            return System.Net.IPAddress.TryParse(ip, out _);
        }
        private async Task HandleRemove()
        {
            if (string.IsNullOrEmpty(IP))
            {
                await JS.InvokeVoidAsync("showAlert", "warning", "Thông báo","Vui lòng nhập IP");
                return;
            }

            var response = await httpClient.PostAsJsonAsync("api/Network/remove-wifi-ip", IP);

            var result = await response.Content.ReadAsStringAsync();

            if (response.IsSuccessStatusCode)
            {
                await JS.InvokeVoidAsync("showAlert", "success", "Thông báo", result);
                return;
            }
            else
            {
                if (response.StatusCode == System.Net.HttpStatusCode.BadRequest)
                {
                    await JS.InvokeVoidAsync("showAlert", "error", "Lỗi", result);
                }
                else
                {
                    await JS.InvokeVoidAsync("showAlert", "error", "Lỗi", result);
                }
            }
        }

        private async Task HandleGetIP()
        {
            try
            {
                var ip = await httpClient.GetStringAsync("api/Network/get-ip-host");

                if(string.IsNullOrEmpty(ip)) { await JS.InvokeVoidAsync("showAlert", "error", "Lỗi", "Lỗi lấy địa chỉ IP"); return; }

                IP = ip;
            }
            catch
            {
                await JS.InvokeVoidAsync("showAlert", "error", "Lỗi", "Không thể lấy IP wifi");
            }
        }
    }
}
