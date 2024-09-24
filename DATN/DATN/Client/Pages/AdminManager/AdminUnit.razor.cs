
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
        private bool isLoaded = false;
        private string errorMessage;

        protected override async Task OnInitializedAsync()
        {
            await LoadUnits();
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

        private async Task DeleteUnit(int unitId)
        {
            try
            {
                var unit = listUnit.FirstOrDefault(p => p.UnitId == unitId);
                if (unit != null)
                {
                    await httpClient.DeleteAsync($"api/Unit/{unitId}");
                    await LoadUnits();
                    StateHasChanged();
                }
            }
            catch (Exception ex)
            {
                errorMessage = $"Error deleting unit: {ex.Message}";
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



    }
}


