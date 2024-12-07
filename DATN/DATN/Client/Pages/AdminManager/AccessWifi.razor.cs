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

            if (response.IsSuccessStatusCode)
            {
                await JS.InvokeVoidAsync("showAlert", "success", "Thông báo","Đã thêm IP.");
                return;
            }
            else
            {
                var errorMessage = await response.Content.ReadAsStringAsync();
                if (response.StatusCode == System.Net.HttpStatusCode.BadRequest)
                {
                    await JS.InvokeVoidAsync("showAlert", "error", "Lỗi", errorMessage);
                }
                else
                {
                    await JS.InvokeVoidAsync("showAlert", "error", "Lỗi", "Đã xảy ra lỗi không xác định.");
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

            if (response.IsSuccessStatusCode)
            {
                await JS.InvokeVoidAsync("showAlert", "success", "Thông báo","Đã xóa IP.");
                return;
            }
            else
            {
                var errorMessage = await response.Content.ReadAsStringAsync();

                if (response.StatusCode == System.Net.HttpStatusCode.BadRequest)
                {
                    await JS.InvokeVoidAsync("showAlert", "error", "Lỗi", errorMessage);
                }
                else
                {
                    await JS.InvokeVoidAsync("showAlert", "error", "Lỗi", "Đã xảy ra lỗi không xác định.");
                }
            }
        }

        private async Task HandleGetIP()
        {
            try
            {
                var ip = await httpClient.GetStringAsync("api/Network/get-ip-host");
                if(string.IsNullOrEmpty(ip)) { await JS.InvokeVoidAsync("showAlert", "error", "Lỗi", "Lỗi Get IP"); return; }
                IP = ip;
            }
            catch
            {
                await JS.InvokeVoidAsync("showAlert", "error", "Lỗi", "Không thể lấy IP wifi");
            }
        }
    }
}
