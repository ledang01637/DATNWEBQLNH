
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
        private List<DATN.Shared.EmployeeShifte> listEmployeeShifte = new List<DATN.Shared.EmployeeShifte>();
        private List<DATN.Shared.Shifte> listShifte = new List<DATN.Shared.Shifte>();
        private List<DATN.Shared.Shifte> filter = new List<DATN.Shared.Shifte>();
        private bool isLoaded = false;
        private string errorMessage;

        protected override async Task OnInitializedAsync()
        {
            await LoadShifte();
            isLoaded = true;
        }

        private async Task LoadShifte()
        {
            try
            {
                listEmployeeShifte = await httpClient.GetFromJsonAsync<List<DATN.Shared.EmployeeShifte>>("api/EmployeeShifte/GetEmployeeShifte");
                listShifte = await httpClient.GetFromJsonAsync<List<DATN.Shared.Shifte>>("api/Shifte/GetShifte");
                filter = listShifte;
            }
            catch (Exception ex)
            {
                errorMessage = $"Error loading unit: {ex.Message}";
            }
        }

        private async Task HideShifte(int shifteId)
        {
            try
            {
                var shifte = listShifte.FirstOrDefault(p => p.Shifte_Id == shifteId);
                if (shifte != null)
                {
                    shifte.IsDeleted = false;
                    await httpClient.PutAsJsonAsync($"api/Shifte/{shifteId}", shifte);
                    await LoadShifte();
                    StateHasChanged();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error hiding voucher: {ex.Message}");
            }
        }

        private async Task RestoreShifte(int shifteId)
        {
            try
            {
                var shifte = listShifte.FirstOrDefault(p => p.Shifte_Id == shifteId);
                if (shifte != null)
                {
                    shifte.IsDeleted = false;
                    await httpClient.PutAsJsonAsync($"api/Shifte/{shifteId}", shifte);
                    await LoadShifte();
                    StateHasChanged();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Đã xảy ra lỗi khi khôi phục : {ex.Message}");
            }
        }

        private void EditShifte(int voucherId)
        {
            Navigation.NavigateTo($"/editshifte/{voucherId}");
        }

        private void CreateShifte()
        {
            Navigation.NavigateTo($"/createshifte");
        }

        private void Filter(ChangeEventArgs e)
        {
            var searchTerm = e.Value.ToString().ToLower();
            filter = string.IsNullOrWhiteSpace(searchTerm)
                ? listShifte
                : listShifte.Where(p => p.Shifte_Name.ToLower().Contains(searchTerm)).ToList();
        }



    }
}


