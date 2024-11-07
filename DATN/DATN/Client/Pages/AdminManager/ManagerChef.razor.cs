using DATN.Shared;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.JSInterop;
using System;
using System.Collections.Generic;
using System.Net.Http.Json;
using System.Threading.Tasks;

namespace DATN.Client.Pages.AdminManager
{
    public partial class ManagerChef
    {
        private HubConnection hubConnection;
        private List<CartDTO> cartDTOs = new();
        private string Note;
        private string numberTable;

        protected override async Task OnInitializedAsync()
        {
            hubConnection = new HubConnectionBuilder()
                .WithUrl(Navigation.ToAbsoluteUri("/ProcessHub"))
                .Build();

            hubConnection.On<string, List<CartDTO>, string>("ReqChef", (_numTable, carts, note) =>
            {
                numberTable = _numTable;
                cartDTOs = carts ?? new List<CartDTO>();
                Note = note ?? string.Empty;

                Console.WriteLine($"Nhận được: {numberTable}, Số lượng carts: {cartDTOs.Count}, Ghi chú: {Note}");
                StateHasChanged();
            });

            await hubConnection.StartAsync();


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
