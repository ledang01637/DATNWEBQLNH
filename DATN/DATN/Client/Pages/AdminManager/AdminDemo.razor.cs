using DATN.Shared;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.JSInterop;
using System.Collections.Generic;
using System.Net.Http.Json;
using System.Net.Http;
using System.Threading.Tasks;
using System;
using System.Linq;
using System.Diagnostics.SymbolStore;

namespace DATN.Client.Pages.AdminManager
{
    public partial class AdminDemo
    {
        private Order getOrder = new Order();
        private List<CartDTO> getCarts = new List<CartDTO>();
        private List<CartDTO> cartsDto = new List<CartDTO>();
        private List<int> numtables = new List<int>();
        private HubConnection hubConnection;
        private bool isProcessing = false;
        private List<Table> tables = new List<Table>();
        private List<Floor> floors = new List<Floor>();
        public DotNetObjectReference<TablePage> dotNetObjectReference;
        private bool isMoveTable = false;
        private int numcol = 6;
        private string getMessage;
        private string getJson;
        private Dictionary<int, List<CartDTO>> cartsByTable = new Dictionary<int, List<CartDTO>>();

        private int rowCount { get; set; }

        protected override async Task OnInitializedAsync()
        {
            hubConnection = new HubConnectionBuilder()
                .WithUrl(Navigation.ToAbsoluteUri("/ProcessHub"))
                .Build();

            hubConnection.On<string>("UpdateAdminDashboard", (message) =>
            {
                isProcessing = true;
                getMessage = message;
                StateHasChanged();
            });

            hubConnection.On<string, List<CartDTO>>("UpdateTable", (numTable, carts) =>
            {
                isProcessing = true;
                getMessage = numTable;
                getCarts = carts;

                if (!string.IsNullOrEmpty(getMessage) && int.TryParse(getMessage, out int tableNumber))
                {
                    if (!numtables.Contains(tableNumber))
                    {
                        numtables.Add(tableNumber);
                    }
                }

                StateHasChanged();
            });

            await hubConnection.StartAsync();

            await LoadAll();
            StateHasChanged();
        }

        private async Task LoadAll()
        {
            try
            {
                tables = await httpClient.GetFromJsonAsync<List<Table>>("api/Table/GetTable");
                if (tables.Any())
                {
                    tables = tables.Where(a => !a.IsDeleted).ToList();
                    CalculateRowCount();
                }
                floors = await httpClient.GetFromJsonAsync<List<Floor>>("api/Floor/GetFloor");
            }
            catch (Exception ex)
            {
                await HandleError(ex);
            }
        }

        private void CalculateRowCount()
        {
            rowCount = (int)Math.Ceiling((double)tables.Count / 6);
            if (tables.Count % 6 == 0) rowCount++;
            Console.WriteLine("rowCount: " + rowCount);
        }

        private int GetRowFromPosition(string position)
        {
            return string.IsNullOrEmpty(position) ? 0 : int.Parse(position.Split('-')[0].Trim().Replace("Hàng", "").Trim());
        }

        public int GetColumnFromPosition(string position)
        {
            return string.IsNullOrEmpty(position) ? 0 : int.Parse(position.Split('-')[1].Trim().Replace("Cột", "").Trim());
        }

        private async Task HandleError(Exception ex)
        {
            var query = $"[C#] fix error: {ex.Message}";
            await JS.InvokeVoidAsync("openChatGPT", query);
            Console.WriteLine($"{ex.Message}");
        }
        private void ShowProductModal(int numberTable)
        {
            var newCarts = getCarts.Where(c => c.TableNumber == numberTable).ToList();

            if (cartsByTable.TryGetValue(numberTable, out var existingCarts))
            {
                foreach (var cartItem in newCarts)
                {
                    var existingItem = existingCarts.FirstOrDefault(c => c.ProductId == cartItem.ProductId);
                    if (existingItem != null)
                    {
                        existingItem.Quantity += cartItem.Quantity;
                    }
                    else
                    {
                        existingCarts.Add(cartItem);
                    }
                }
            }
            else
            {
                cartsByTable[numberTable] = newCarts;
            }

            cartsDto = cartsByTable[numberTable];

            StateHasChanged();
        }

        private async void ConfirmOrder()
        {
            await JS.InvokeVoidAsync("showAlert", "success", "Đã gửi đầu bếp");
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
