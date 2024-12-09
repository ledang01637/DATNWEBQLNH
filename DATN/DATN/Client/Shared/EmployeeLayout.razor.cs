using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.JSInterop;
using System.Net.Http;
using System.Threading.Tasks;

namespace DATN.Client.Shared
{
    public partial class EmployeeLayout
    {
        private string wifiIpAddress;
        private bool isAcceptWifi;
        private bool isLoading;
        private string Username;
        private bool isCheckBookTable = false;
        private bool isShow = false;

        protected override async Task OnInitializedAsync()
        {

            isLoading = true;
            try
            {
                wifiIpAddress = await httpClient.GetStringAsync("api/Network/get-ip-host");
                if (string.IsNullOrEmpty(wifiIpAddress))
                {
                    isAcceptWifi = false;
                    return;
                }
                isAcceptWifi = true;
                Username = await _localStorageService.GetItemAsync("userName");
            }
            catch
            {
                isAcceptWifi = false;
                await JS.InvokeVoidAsync("showAlert", "error", "Lỗi", "Không thể lấy IP wifi");
            }
            finally
            {
                isLoading = false;
            }
        }
        private async void Navbar(bool isClose)
        {
            await JS.InvokeVoidAsync("Navbar", "overlay", "mySidebar", isClose);
        }

        private void GoToManager(bool isCheck)
        {
            isShow = isCheck;
            string query = "?isCheckBookTable="+ isCheck;
            Navigation.NavigateTo("/manager" + query,true);
        }

        private void Logout()
        {
            Navigation.NavigateTo("/logout");
        }

        public async ValueTask DisposeAsync()
        {
            await JS.InvokeVoidAsync("stopSnowfall");
        }
    }
}
