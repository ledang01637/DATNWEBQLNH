
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
    public partial class AdminEmployee
    {
        private List<DATN.Shared.Employee> listEmployee = new List<DATN.Shared.Employee>();
        private List<DATN.Shared.EmployeeShifte> listEmployeeShifte = new List<DATN.Shared.EmployeeShifte>();
        private List<DATN.Shared.Shifte> listShifte = new List<DATN.Shared.Shifte>();
        private List<DATN.Shared.Employee> filter = new List<DATN.Shared.Employee>();
        private bool isLoaded = false;
        private string errorMessage;

        protected override async Task OnInitializedAsync()
        {
            await Load();
            isLoaded = true;
        }

        private async Task Load()
        {
            try
            {
                listShifte = await httpClient.GetFromJsonAsync<List<DATN.Shared.Shifte>>("api/Shifte/GetShifte");
                listEmployeeShifte = await httpClient.GetFromJsonAsync<List<DATN.Shared.EmployeeShifte>>("api/EmployeeShifte/GetEmployeeShifte");
                listEmployee = await httpClient.GetFromJsonAsync<List<DATN.Shared.Employee>>("api/Employee/GetEmployee");
                filter = listEmployee;
            }
            catch (Exception ex)
            {
                errorMessage = $"Error loading Employee: {ex.Message}";
            }
        }

        private async Task DeleteEmployee(int employeeId)
        {
            try
            {
                var employee = listEmployee.FirstOrDefault(p => p.EmployeeId == employeeId);
                if (employee != null)
                {
                    await httpClient.DeleteAsync($"api/Employee/{employeeId}");
                    await Load();
                    StateHasChanged();
                }
            }
            catch (Exception ex)
            {
                errorMessage = $"Error deleting employee: {ex.Message}";
            }
        }

        private void EditEmployee(int employeeId)
        {
            Navigation.NavigateTo($"/editemployee/{employeeId}");
        }

        private void CreateEmployee()
        {
            Navigation.NavigateTo($"/createemployee");
        }

        private void FilterEmployee(ChangeEventArgs e)
        {
            var searchTerm = e.Value.ToString().ToLower();
            filter = string.IsNullOrWhiteSpace(searchTerm)
                ? listEmployee
                : listEmployee.Where(p => p.EmployeeName.ToLower().Contains(searchTerm)).ToList();
        }

    }
}
