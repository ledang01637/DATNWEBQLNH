using DATN.Shared;
using Microsoft.JSInterop;
using System.Net.Http;
using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Components;

namespace DATN.Client.Pages
{
    public partial class TablePage
    {
        private Table tableModel = new Table();
        private List<Table> tables = new List<Table>();
        private List<Floor> floors = new List<Floor>();
        private int selectTableId;
        private int rowCount = 0;
        private bool isMoveTable = false;
        public string row { get; set; }
        public string column { get; set; }

        protected override async Task OnInitializedAsync()
        {
            await LoadAll();
        }
        private async Task LoadAll()
        {
            try
            {
                tables = await httpClient.GetFromJsonAsync<List<Table>>("api/Table/GetTable");
                if(tables.Count > 0)
                {
                    tables = tables.Where(a => a.IsDeleted.Equals(false)).ToList();
                    rowCount = (int)Math.Ceiling((double)tables.Count / 6);
                }
                floors = await httpClient.GetFromJsonAsync<List<Floor>>("api/Floor/GetFloor");
            }
            catch (Exception ex)
            {
                var query = $"[C#] fix error: {ex.Message}";
                await JS.InvokeVoidAsync("openChatGPT", query);
            }
        }
        private async Task AddTable()
        {
            try
            {
                var existingTable = tables.FirstOrDefault(r => r.TableNumber == tableModel.TableNumber);
                if (existingTable != null)
                {
                    await JS.InvokeVoidAsync("showAlert", "error","Lỗi","Số bàn đã tồn tại");
                    await Task.Delay(1000);
                    return;
                }
                tableModel.IsDeleted = false;
                tableModel.Status = "Bàn trống";
                tableModel.Position = $"{row} - {column}";
                var response = await httpClient.PostAsJsonAsync("api/Table/AddTable", tableModel);

                if (response.IsSuccessStatusCode)
                {
                    await JS.InvokeVoidAsync("showAlert", "success","Thành công","");
                    await LoadAll();
                    StateHasChanged();
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
            StateHasChanged();
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
                    await JS.InvokeVoidAsync("showAlert", "TableSuccess");
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
                    await JS.InvokeVoidAsync("showAlert", "TableSuccess");
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
            row = e.Value.ToString();
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
    }
}
