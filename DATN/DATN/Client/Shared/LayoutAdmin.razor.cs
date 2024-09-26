using DATN.Shared;
using System;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;

namespace DATN.Client.Shared
{
    public partial class LayoutAdmin
    {
        private LoginRequest loginRequest = new LoginRequest();
        private string wifiIpAddress;
        private bool isAcceptWifi;

        protected override async Task OnInitializedAsync()
        {
            wifiIpAddress = await httpClient.GetStringAsync("api/Network/wifi-ip");
            if (wifiIpAddress.Equals("192.168.218.1") || wifiIpAddress.Equals("192.168.1.87"))
            {
                isAcceptWifi = true;
                loginRequest.Username = "Customer";
                loginRequest.Password = "123@#$";
                var respone = await httpClient.PostAsJsonAsync("api/AuthJWT/AuthUser", loginRequest);
                if (respone.IsSuccessStatusCode)
                {
                    Console.Write("abc");
                }
            }
            else
            {
                isAcceptWifi = false;
            }
        }
    }
}
