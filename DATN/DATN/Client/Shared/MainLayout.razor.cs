using DATN.Shared;
using Microsoft.AspNetCore.Components;
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
        public string name { get; set; } = string.Empty;

        EventCallback<string> even_callback => EventCallback.Factory.Create(this, (Action<string>)NontifyLayoutWithDataCustomerName);



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
                Console.WriteLine(errorMessage);
            }
            finally
            {
                isLoading = false;
            }
        }

        public void NontifyLayoutWithDataCustomerName(string  _name)
        {
            string[] nameParts = _name.Split(' ');

            string lastName = nameParts[nameParts.Length - 1];

            name = lastName;
        }

    }


}
