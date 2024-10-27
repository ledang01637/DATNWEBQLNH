using DATN.Shared;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.JSInterop;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Json;
using System.Threading.Tasks;

namespace DATN.Client.Pages
{
    public partial class Demo
    {
        private HubConnection hubConnection;
        private string message;
        private int searchTerm = 0;
        private bool showModal = false;
        private List<Table> tables = new();
        private List<Floor> floors = new();
        private List<RequestTable> requestsTable = new();

        protected override async Task OnInitializedAsync()
        {
            hubConnection = new HubConnectionBuilder()
                .WithUrl(Navigation.ToAbsoluteUri("/ProcessHub"))
                .Build();

            tables = await httpClient.GetFromJsonAsync<List<Table>>("api/Table/GetTable");
            floors = await httpClient.GetFromJsonAsync<List<Floor>>("api/Floor/GetFloor");
            
            if(tables.Count > 0 && floors.Count > 0)
            {
                foreach(var table in tables)
                {
                    requestsTable.Add(new RequestTable
                    {
                        TableId = table.TableId,
                        NumberTable = table.TableNumber,
                        FloorId = table.FloorId,
                        IsCompleted = false
                    });
                }
            }

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
                await JS.InvokeVoidAsync("howAlert", "Không thể kết nối tới server!");
            }
        }

        private void CompleteTableRequest(RequestTable table)
        {
            table.IsCompleted = true;
            showModal = true;
        }

        private void CloseModal()
        {
            showModal = false;
        }

        // Function to filter tables based on search term
        private IEnumerable<RequestTable> FilterTables(IEnumerable<RequestTable> requestTables)
        {
           
            if (searchTerm == 0)
            {
                return requestTables;
            }

            return requestTables.Where(t => t.NumberTable == searchTerm);
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
