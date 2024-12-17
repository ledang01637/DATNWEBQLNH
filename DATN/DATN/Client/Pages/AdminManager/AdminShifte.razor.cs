
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
        private List<DATN.Shared.EmployeeShifte> listEmployeeShifte = new List<DATN.Shared.EmployeeShifte>();
        private List<DATN.Shared.Employee> listEmployee = new List<DATN.Shared.Employee>();
        private List<DATN.Shared.Shifte> filter = new List<DATN.Shared.Shifte>();
        private List<DATN.Shared.EmployeeShifte> filteremployee = new List<DATN.Shared.EmployeeShifte>();
        private bool isLoaded = false;
        private string errorMessage;

        protected override async Task OnInitializedAsync()
        {
            try
            {
                await LoadEmployeeShiftes();
                await LoadShiftes();
            }
            finally
            {
                isLoaded = true;
            }
        }

        private async Task LoadEmployeeShiftes()
        {
            try
            {
                listEmployeeShifte = await httpClient.GetFromJsonAsync<List<DATN.Shared.EmployeeShifte>>("api/EmployeeShifte/GetEmployeeShifte");
                listEmployee = await httpClient.GetFromJsonAsync<List<DATN.Shared.Employee>>("api/Employee/GetEmployee");
                listShifte = await httpClient.GetFromJsonAsync<List<DATN.Shared.Shifte>>("api/Shifte/GetShifte");
                filteremployee = listEmployeeShifte;
            }
            catch (Exception ex)
            {
                errorMessage = $"Error loading unit: {ex.Message}";
            }
        }

        private async Task LoadShiftes()
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

        //shifte
        private async Task HideShifte(int shifteId)
        {
            try
            {
                var shifte = listShifte.FirstOrDefault(p => p.ShifteId == shifteId);
                if (shifte != null)
                {
                    shifte.IsDeleted = true;
                    await httpClient.PutAsJsonAsync($"api/Shifte/{shifteId}", shifte);
                    await LoadShiftes();
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
                var shifte = listShifte.FirstOrDefault(p => p.ShifteId == shifteId);
                if (shifte != null)
                {
                    shifte.IsDeleted = false;
                    await httpClient.PutAsJsonAsync($"api/Shifte/{shifteId}", shifte);
                    await LoadShiftes();
                    StateHasChanged();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Đã xảy ra lỗi khi khôi phục : {ex.Message}");
            }
        }
        private void EditShifte(int shifteId)
        {
            Navigation.NavigateTo($"/admin/editshifte/{shifteId}");
        }
        private void CreateShifte()
        {
            Navigation.NavigateTo($"/admin/createshifte");
        }

        //employeeshifte
        private async Task HideEmployeeShifte(int employeeShifteId)
        {
            try
            {
                var employeeShifte = listEmployeeShifte.FirstOrDefault(p => p.EmployeeShifteId == employeeShifteId);
                if (employeeShifte != null)
                {
                    employeeShifte.IsDeleted = true;
                    await httpClient.PutAsJsonAsync($"api/EmployeeShifte/{employeeShifteId}", employeeShifte);
                    await LoadEmployeeShiftes();
                    StateHasChanged();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error hiding voucher: {ex.Message}");
            }
        }
        private async Task RestoreEmployeeShifte(int employeeShifteId)
        {
            try
            {
                var employeeShifte = listEmployeeShifte.FirstOrDefault(p => p.EmployeeShifteId == employeeShifteId);
                if (employeeShifte != null)
                {
                    employeeShifte.IsDeleted = false;
                    await httpClient.PutAsJsonAsync($"api/EmployeeShifte/{employeeShifteId}", employeeShifte);
                    await LoadEmployeeShiftes();
                    StateHasChanged();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Đã xảy ra lỗi khi khôi phục : {ex.Message}");
            }
        }
        private void EditEmployeeShifte(int employeeShifteId)
        {
            Navigation.NavigateTo($"/admin/editemployeeShifte/{employeeShifteId}");
        }
        private void CreateEmployeeShifte()
        {
            Navigation.NavigateTo($"/admin/createemployeeShifte");
        }



        private void Filter(ChangeEventArgs e)
        {
            var searchTerm = e.Value.ToString().ToLower();
            filter = string.IsNullOrWhiteSpace(searchTerm)
                ? listShifte
                : listShifte.Where(p => p.ShifteName.ToLower().Contains(searchTerm)).ToList();
        }

        private void FilterEmployee(ChangeEventArgs e)
        {
            var searchTermemployee = e.Value.ToString().ToLower();
            filteremployee = string.IsNullOrWhiteSpace(searchTermemployee)
                ? listEmployeeShifte
                : listEmployeeShifte.Where(p => p.EmployeeShifteId.Equals(searchTermemployee)).ToList();
        }



    }
}


