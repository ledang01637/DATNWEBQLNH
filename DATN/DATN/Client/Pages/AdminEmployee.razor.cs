
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using System;
using System.Net.Http.Json;
using DATN.Shared;
using Microsoft.AspNetCore.Components;
using System.Linq;
using Microsoft.EntityFrameworkCore.Metadata.Internal;

namespace DATN.Client.Pages
{
    public partial class AdminEmployee
    {
        private List<DATN.Shared.Employee> list = new List<DATN.Shared.Employee>();
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
                list = await httpClient.GetFromJsonAsync<List<DATN.Shared.Employee>>("api/Employee/GetEmployee");
                filter = list;
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
                var employee = list.FirstOrDefault(p => p.EmployeeId == employeeId);
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
                ? list
                : list.Where(p => p.EmployeeName.ToLower().Contains(searchTerm)).ToList();
        }

    }
}
