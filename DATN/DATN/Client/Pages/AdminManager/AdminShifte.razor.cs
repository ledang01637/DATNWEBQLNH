
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
    public partial class AdminShifte
    {
        private List<DATN.Shared.Shifte> listShifte = new List<DATN.Shared.Shifte>();
        private List<DATN.Shared.Shifte> filter = new List<DATN.Shared.Shifte>();
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
                listShifte = await httpClient.GetFromJsonAsync<List<DATN.Shared.Shifte>>("api/Shifte/GetShifte");
                filter = listShifte;
            }
            catch (Exception ex)
            {
                errorMessage = $"Error loading unit: {ex.Message}";
            }

        }

        private async Task DeleteShifte(int shifteId)
        {
            try
            {
                var shifte = listShifte.FirstOrDefault(p => p.Shifte_Id == shifteId);
                if (shifte != null)
                {
                    await httpClient.DeleteAsync($"api/Unit/{shifteId}");
                    await LoadUnits();
                    StateHasChanged();
                }
            }
            catch (Exception ex)
            {
                errorMessage = $"Error deleting unit: {ex.Message}";
            }
        }

        private void EditShifte(int shifteId)
        {
            Navigation.NavigateTo($"/editunit/{shifteId}");
        }

        private void CreateShifte()
        {
            Navigation.NavigateTo($"/createunit");
        }


        private void Filter(ChangeEventArgs e)
        {
            var searchTerm = e.Value?.ToString().ToLower() ?? string.Empty;
            filter = string.IsNullOrWhiteSpace(searchTerm)
                ? listShifte
                : listShifte.Where(p => p.Shifte_Name.ToLower().Contains(searchTerm)).ToList();
        }




    }
}


