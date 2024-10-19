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
            if (string.IsNullOrEmpty(IP))
            {
                await JS.InvokeVoidAsync("showAlert", "warning", "Vui lòng nhập IP");
                return;
            }

            var response = await httpClient.PostAsJsonAsync("api/Network/post-wifi-ip", IP);

            if (response.IsSuccessStatusCode)
            {
                await JS.InvokeVoidAsync("showAlert", "success", "Đã thêm IP.");
                return;
            }
            else
            {
                var errorMessage = await response.Content.ReadAsStringAsync();

                if (response.StatusCode == System.Net.HttpStatusCode.BadRequest)
                {
                    await JS.InvokeVoidAsync("showAlert", "error", errorMessage);
                }
                else
                {
                    await JS.InvokeVoidAsync("showAlert", "error", "Đã xảy ra lỗi không xác định.");
                }
            }
        }
        private async Task HandleRemove()
        {
            if (string.IsNullOrEmpty(IP))
            {
                await JS.InvokeVoidAsync("showAlert", "warning", "Vui lòng nhập IP");
                return;
            }

            var response = await httpClient.PostAsJsonAsync("api/Network/remove-wifi-ip", IP);

            if (response.IsSuccessStatusCode)
            {
                await JS.InvokeVoidAsync("showAlert", "success", "Đã xóa IP.");
                return;
            }
            else
            {
                var errorMessage = await response.Content.ReadAsStringAsync();

                if (response.StatusCode == System.Net.HttpStatusCode.BadRequest)
                {
                    await JS.InvokeVoidAsync("showAlert", "error", errorMessage);
                }
                else
                {
                    await JS.InvokeVoidAsync("showAlert", "error", "Đã xảy ra lỗi không xác định.");
                }
            }
        }

    }
}
