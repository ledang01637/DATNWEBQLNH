using DATN.Shared;
using System;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;

namespace DATN.Client.Shared
{
    public partial class MainLayout
    {
        private string wifiIpAddress;
        private bool isAcceptWifi;
        private string errorMessage;
        private bool isLoading;

        protected override async Task OnInitializedAsync()
        {
            isLoading = true;
            try
            {
                wifiIpAddress = await httpClient.GetStringAsync("api/Network/wifi-ip");
                if(string.IsNullOrEmpty(wifiIpAddress)) 
                { 
                    isAcceptWifi = false; 
                    return; 
                }
                isAcceptWifi = true;
            }
            catch (HttpRequestException ex)
            {
                isAcceptWifi = false;
                errorMessage = "Access denied: " + ex.Message;
            }
            finally
            {
                isLoading = false;
            }
        }
    }

}
