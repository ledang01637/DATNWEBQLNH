using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.JSInterop;
using System.Net.Http.Json;
using System.Threading.Tasks;

namespace DATN.Client.Pages
{
    public partial class Demo
    {
        private HubConnection hubConnection;
        private string message;

        protected override async Task OnInitializedAsync()
        {
            hubConnection = new HubConnectionBuilder()
                .WithUrl(Navigation.ToAbsoluteUri("/ProcessHub"))
                .Build();

            await hubConnection.StartAsync();
        }

        private async Task ProcessPayment()
        {
            if (hubConnection is not null && hubConnection.State == HubConnectionState.Connected)
            {
                await hubConnection.SendAsync("SendMessage", message);
            }
            else
            {
                await JS.InvokeVoidAsync("alert", "Không thể kết nối tới server!");
            }
        }


        public async ValueTask DisposeAsync()
        {
            if (hubConnection is not null)
            {
                await hubConnection.DisposeAsync();
            }
        }
    }
}
