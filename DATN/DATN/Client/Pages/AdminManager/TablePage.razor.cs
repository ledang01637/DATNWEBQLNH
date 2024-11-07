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
        private Table tableModel = new();
        private List<Table> tables = new();
        private List<Table> tablesChanges = new();
        private List<Floor> floors = new();
        private QR qrModel = new();
        public DotNetObjectReference<TablePage> dotNetObjectReference;
        private int selectTableId;
        private bool isMoveTable = false;
        private int numcol = 6;
        private int rowCount { get; set; }
        private string row { get; set; }
        private string column { get; set; }


        protected override async Task OnInitializedAsync()
        {
            dotNetObjectReference = DotNetObjectReference.Create(this);
            qrModel.Url = "https://localhost:44328/";
            await LoadAll();
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
        }

        private async Task AddTable()
        {
            if (await ValidateTableExists(tableModel.TableNumber)) return;

            try
            {
                SetDefaultTableProperties();
                var response = await httpClient.PostAsJsonAsync("api/Table/AddTable", tableModel);
                await HandleResponse(response, "Thành công", "Thêm bàn thành công");
            }
            catch (Exception ex)
            {
                await HandleError(ex);
            }
        }

        private async Task<bool> ValidateTableExists(int tableNumber)
        {
            var existingTable = tables.FirstOrDefault(r => r.TableNumber == tableNumber);
            if (existingTable != null)
            {
                await JS.InvokeVoidAsync("showAlert", "error", "Lỗi", "Số bàn đã tồn tại");
                await Task.Delay(1000);
                return true;
            }
            return false;
        }

        private void SetDefaultTableProperties()
        {
            tableModel.IsDeleted = false;
            tableModel.Status = "Bàn trống";
            tableModel.Position = $"{row} - {column}";
        }

        private async Task HandleResponse(HttpResponseMessage response, string successMessage, string errorMessage)
        {
            if (response.IsSuccessStatusCode)
            {
                await JS.InvokeVoidAsync("showAlert", "success", "Thành công", successMessage);
                await LoadAll();
            }
            else
            {
                await JS.InvokeVoidAsync("showAlert", "error", "Lỗi", errorMessage);
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
                SetDefaultTableProperties();
                var response = await httpClient.PutAsJsonAsync($"api/Table/{selectTableId}", tableModel);
                await HandleResponse(response, "Thành công", "Error update Table");
                isMoveTable = false;
            }
            catch (Exception ex)
            {
                await HandleError(ex);
            }
        }

        private async Task DeleteTable()
        {
            try
            {
                await JS.InvokeVoidAsync("closeModal", "ConfirmDeleteModal");
                tableModel.IsDeleted = true;
                await UpdateTable(); // Tái sử dụng logic cập nhật cho việc xóa
            }
            catch (Exception ex)
            {
                await HandleError(ex);
            }
        }

        private int GetRowFromPosition(string position)
        {
            return string.IsNullOrEmpty(position) ? 0 : int.Parse(position.Split('-')[0].Trim().Replace("Hàng", "").Trim());
        }

        public int GetColumnFromPosition(string position)
        {
            return string.IsNullOrEmpty(position) ? 0 : int.Parse(position.Split('-')[1].Trim().Replace("Cột", "").Trim());
        }

        private async Task GetPositionTable(int floorId, bool isSwap)
        {
            await JS.InvokeVoidAsync("MoveTable", floorId, isSwap, dotNetObjectReference, numcol);
        }

        private async Task ActivateMoveTable(bool isSwap)
        {
            isMoveTable = true;
            foreach (var floor in floors)
            {
                await GetPositionTable(floor.FloorId, isSwap);
            }
        }

        private async Task SaveTable()
        {
            foreach (var tableChange in tablesChanges)
            {
                await httpClient.PutAsJsonAsync($"api/Table/{tableChange.TableId}", tableChange);
            }
            tablesChanges.Clear();
            isMoveTable = false;
            Navigation.NavigateTo("edittable", true);
        }

        [JSInvokable("MoveFloor")]
        public void MoveFloor(int tableId, string newPosition, int newFloorId)
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
        [JSInvokable("UpdateTablePosition")]
        public void UpdateTablePosition(int tableId, string newPosition, int newFloorId, int swappedTableId, string swappedPosition)
        {
            var currentTable = tables.FirstOrDefault(t => t.TableId == tableId);
            if (currentTable != null)
            {
                currentTable.Position = newPosition;
                currentTable.FloorId = newFloorId;

                tablesChanges.Add(new Table
                {
                    TableId = tableId,
                    Position = newPosition,
                    FloorId = newFloorId,
                    TableNumber = currentTable.TableNumber,
                    SeatingCapacity = currentTable.SeatingCapacity,
                    IsDeleted = currentTable.IsDeleted,
                    Status = currentTable.Status
                });
            }

            var swappedTable = tables.FirstOrDefault(t => t.TableId == swappedTableId);
            if (swappedTable != null)
            {
                swappedTable.Position = swappedPosition;

                tablesChanges.Add(new Table
                {
                    TableId = swappedTableId,
                    Position = swappedPosition,
                    FloorId = swappedTable.FloorId,
                    TableNumber = swappedTable.TableNumber,
                    SeatingCapacity = swappedTable.SeatingCapacity,
                    IsDeleted = swappedTable.IsDeleted,
                    Status = swappedTable.Status
                });
            }
        }

        private async Task GenerateQrCode()
        {
            var mD5Hash = await GenerateMD5Hash(qrModel.NumberTable.ToString());
            var urlCode = $"{qrModel.Url}demoIndex?n={mD5Hash}";

            await _localStorageService.SetItemAsync("ss", urlCode);
            await JS.InvokeVoidAsync("clearQrCode");
            await JS.InvokeVoidAsync("generateQrCode", urlCode);
        }
        private async Task<string> GenerateMD5Hash(string input)
        {
            return await JS.InvokeAsync<string>("generateMD5Hash", input);
        }

        private async Task HandleError(Exception ex)
        {
            var query = $"[C#] fix error: {ex.Message}";
            await JS.InvokeVoidAsync("openChatGPT", query);
            Console.WriteLine($"{ex.Message}");
        }

        public void Dispose()
        {
            dotNetObjectReference?.Dispose();
        }

    }
}
