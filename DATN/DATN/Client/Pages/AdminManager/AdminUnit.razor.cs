
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using System;
using System.Net.Http.Json;
using DATN.Shared;
using Microsoft.AspNetCore.Components;
using System.Linq;
using Microsoft.EntityFrameworkCore.Metadata.Internal;

namespace DATN.Client.Pages.AdminManager
{
    public partial class AdminUnit
    {
        private List<DATN.Shared.Unit> listUnit = new List<DATN.Shared.Unit>();
        private List<DATN.Shared.Unit> filter = new List<DATN.Shared.Unit>();
        //

        private List<DATN.Shared.Floor> listFloor = new List<DATN.Shared.Floor>();
        private List<DATN.Shared.Floor> filterfloor = new List<DATN.Shared.Floor>();
        private bool isLoaded = false;
        private string errorMessage;

        protected override async Task OnInitializedAsync()
        {
            await LoadUnits();
            await LoadFloors();
            isLoaded = true;
        }

        private async Task LoadUnits()
        {
            try
            {
                listUnit = await httpClient.GetFromJsonAsync<List<DATN.Shared.Unit>>("api/Unit/GetUnit");
                filter = listUnit;
            }
            catch (Exception ex)
            {
                errorMessage = $"Error loading unit: {ex.Message}";
            }
        }

        private async Task HideUnit(int unitId)
        {
            try
            {
                var unit = listUnit.FirstOrDefault(p => p.UnitId == unitId);
                if (unit != null)
                {
                    unit.IsDeleted = true;
                    await httpClient.PutAsJsonAsync($"api/Unit/{unitId}", unit);
                    await LoadUnits();
                    StateHasChanged();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error hiding voucher: {ex.Message}");
            }
        }
        private async Task RestoreUnit(int unitId)
        {
            try
            {
                var unit = listUnit.FirstOrDefault(p => p.UnitId == unitId);
                if (unit != null)
                {
                    unit.IsDeleted = false;
                    await httpClient.PutAsJsonAsync($"api/Unit/{unitId}", unit);
                    await LoadUnits();
                    StateHasChanged();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Đã xảy ra lỗi khi khôi phục : {ex.Message}");
            }
        }

        private void EditUnit(int unitId)
        {
            Navigation.NavigateTo($"/editunit/{unitId}");
        }

        private void CreateUnit()
        {
            Navigation.NavigateTo($"/createunit");
        }

        private void Filter(ChangeEventArgs e)
        {
            var searchTerm = e.Value.ToString().ToLower();
            filter = string.IsNullOrWhiteSpace(searchTerm)
                ? listUnit
                : listUnit.Where(p => p.UnitName.ToLower().Contains(searchTerm)).ToList();
        }

        //


        private async Task LoadFloors()
        {
            try
            {
                listFloor = await httpClient.GetFromJsonAsync<List<DATN.Shared.Floor>>("api/Floor/GetFloor");
                filterfloor = listFloor;
            }
            catch (Exception ex)
            {
                errorMessage = $"Error loading unit: {ex.Message}";
            }
        }

        private async Task HideFloor(int floorId)
        {
            try
            {
                var floor = listFloor.FirstOrDefault(p => p.FloorId == floorId);
                if (floor != null)
                {
                    floor.IsDeleted = true;
                    await httpClient.PutAsJsonAsync($"api/Floor/{floorId}", floor);
                    await LoadFloors();
                    StateHasChanged();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error hiding voucher: {ex.Message}");
            }
        }
        private async Task RestoreFloor(int floorId)
        {
            try
            {
                var floor = listFloor.FirstOrDefault(p => p.FloorId == floorId);
                if (floor != null)
                {
                    floor.IsDeleted = false;
                    await httpClient.PutAsJsonAsync($"api/Floor/{floorId}", floor);
                    await LoadFloors();
                    StateHasChanged();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Đã xảy ra lỗi khi khôi phục : {ex.Message}");
            }
        }

        private void EditFloor(int floorId)
        {
            Navigation.NavigateTo($"/editfloor/{floorId}");
        }

        private void CreateFloor()
        {
            Navigation.NavigateTo($"/createfloor");
        }

        private void FilterFloor(ChangeEventArgs e)
        {
            var searchTermfloor = e.Value.ToString().ToLower();
            filterfloor = string.IsNullOrWhiteSpace(searchTermfloor)
                ? listFloor
                : listFloor.Where(p => p.NumberFloor.Equals(searchTermfloor)).ToList();
        }

    }
}


