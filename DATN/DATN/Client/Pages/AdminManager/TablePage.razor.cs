using DATN.Shared;
using Microsoft.JSInterop;
using System.Net.Http;
using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Components;
using System.Drawing;

namespace DATN.Client.Pages.AdminManager
{
    public partial class TablePage
    {
        private Table tableModel = new Table();
        private List<Table> tables = new List<Table>();
        private List<Table> tablesChanges = new List<Table>();
        private List<Floor> floors = new List<Floor>();
        public DotNetObjectReference<TablePage> dotNetObjectReference;
        private int selectTableId;
        private bool isMoveTable = false;
        private int rowCount { get; set; }
        private string row { get; set; }
        private string column { get; set; }


        protected override async Task OnInitializedAsync()
        {
            dotNetObjectReference = DotNetObjectReference.Create(this);
            await LoadAll();
        }
        private async Task LoadAll()
        {
            try
            {
                tables = await httpClient.GetFromJsonAsync<List<Table>>("api/Table/GetTable");
                if (tables.Count > 0)
                {
                    tables = tables.Where(a => a.IsDeleted.Equals(false)).ToList();
                    CalculateRowCount();
                }
                floors = await httpClient.GetFromJsonAsync<List<Floor>>("api/Floor/GetFloor");
            }
            catch (Exception ex)
            {
                var query = $"[C#] fix error: {ex.Message}";
                await JS.InvokeVoidAsync("openChatGPT", query);
            }
        }
        private void CalculateRowCount()
        {
            rowCount = (int)Math.Ceiling((double)tables.Count / 6);

            if (tables.Count() % 6 == 0)
            {
                rowCount += 1;
                Console.WriteLine("rowCount" + rowCount);
            }
        }
        private async Task AddTable()
        {
            try
            {
                var existingTable = tables.FirstOrDefault(r => r.TableNumber == tableModel.TableNumber);
                if (existingTable != null)
                {
                    await JS.InvokeVoidAsync("showAlert", "error", "Lỗi", "Số bàn đã tồn tại");
                    await Task.Delay(1000);
                    return;
                }
                Console.Write("a", tableModel);
                tableModel.IsDeleted = false;
                tableModel.Status = "Bàn trống";
                tableModel.Position = $"{row} - {column}";
                var response = await httpClient.PostAsJsonAsync("api/Table/AddTable", tableModel);

                if (response.IsSuccessStatusCode)
                {
                    await JS.InvokeVoidAsync("showAlert", "success", "Thành công", "");
                    await LoadAll();
                }
                else
                {
                    Console.WriteLine("Error add Table");
                }
            }
            catch (Exception ex)
            {
                var query = $"[C#] fix error: {ex.Message}";
                await JS.InvokeVoidAsync("openChatGPT", query);
                Console.WriteLine($"{ex.Message}");

            }
        }
        private async Task LoadTableForEdit(int tableId)
        {
            tableModel = await httpClient.GetFromJsonAsync<Table>($"api/Table/{tableId}");
            selectTableId = tableModel.TableId;
            var parts = tableModel.Position.Split('-');
            row = parts[0].Trim();
            column = parts[1].Trim();
            isMoveTable = true;
            await JS.InvokeVoidAsync("MoveTable");
        }
        private async Task UpdateTable()
        {
            try
            {
                tableModel.IsDeleted = false;
                tableModel.Position = $"{row} - {column}";
                var response = await httpClient.PutAsJsonAsync($"api/Table/{selectTableId}", tableModel);

                if (response.IsSuccessStatusCode)
                {
                    await JS.InvokeVoidAsync("showAlert", "success", "Thành công", "");
                    isMoveTable = false;
                    await LoadAll();
                }
                else
                {
                    Console.WriteLine("Error update Table");
                }
            }
            catch (Exception ex)
            {
                var query = $"[C#] fix error: {ex.Message}";
                await JS.InvokeVoidAsync("openChatGPT", query);
                Console.WriteLine($"{ex.Message}");
            }
        }
        private async Task DeleteTable()
        {
            try
            {
                await JS.InvokeVoidAsync("closeModal", "ConfirmDeleteModal");
                tableModel.IsDeleted = true;
                tableModel.Position = $"{row} - {column}";
                var response = await httpClient.PutAsJsonAsync($"api/Table/{selectTableId}", tableModel);

                if (response.IsSuccessStatusCode)
                {
                    await JS.InvokeVoidAsync("showAlert", "success", "Thành công", "");
                    isMoveTable = false;
                    await LoadAll();
                }
                else
                {
                    Console.WriteLine("Error delete Table");
                }
            }
            catch (Exception ex)
            {
                var query = $"[C#] fix error: {ex.Message}";
                await JS.InvokeVoidAsync("openChatGPT", query);
                Console.WriteLine($"{ex.Message}");
            }
        }
        private void OnFloorChanged(ChangeEventArgs e)
        {
            tableModel.FloorId = int.Parse(e.Value.ToString());
        }
        private void OnRawChange(ChangeEventArgs e)
        {
            var newRow = e.Value.ToString();
            if (newRow != row)
            {
                row = newRow;
            }
        }
        private void OnColumnChange(ChangeEventArgs e)
        {
            column = e.Value.ToString();
        }
        private int GetRowFromPosition(string position)
        {
            if (string.IsNullOrEmpty(position))
            {
                return 0;
            }
            var parts = position.Split('-');
            return int.Parse(parts[0].Trim().Replace("Hàng", "").Trim());
        }
        public int GetColumnFromPosition(string position)
        {
            if (string.IsNullOrEmpty(position))
            {
                return 0;
            }
            var parts = position.Split('-');
            return int.Parse(parts[1].Trim().Replace("Cột", "").Trim());
        }
        private async Task GetPosionTable(int FloorId, bool IsSwap)
        {
            await JS.InvokeVoidAsync("MoveTable", FloorId, IsSwap,dotNetObjectReference);
        }
        private async Task AcctiveMoveTable(bool _isSwap)
        {
            isMoveTable = true;
            foreach (var f in floors)
            {
               await GetPosionTable(f.FloorId, _isSwap);
            }
        }

        private async Task SaveTable()
        {
            for (int i = 0; i < tablesChanges.Count; i++)
            {
                var tableChange = tablesChanges[i];
                await httpClient.PutAsJsonAsync($"api/Table/{tableChange.TableId}", tableChange);
            }
            tablesChanges.Clear();
            isMoveTable = false;
        }

        [JSInvokable("UpdateTablePosition")]
        public void UpdateTablePosition(int tableId, string newPosition, int newFloorId)
        {
            var table = tables.FirstOrDefault(t => t.TableId == tableId);

            if (table != null)
            {
                table.Position = newPosition;
                tablesChanges.Add(new Table
                {
                    TableId = tableId,
                    Position = newPosition,
                    FloorId = newFloorId,
                    TableNumber = table.TableNumber,
                    SeatingCapacity = table.SeatingCapacity,
                    IsDeleted = table.IsDeleted,
                    Status = table.Status
                });
            }

        }
    }
}
