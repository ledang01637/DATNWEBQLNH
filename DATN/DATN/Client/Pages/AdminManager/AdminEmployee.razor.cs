
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
        private List<DATN.Shared.Account> listAccount = new List<DATN.Shared.Account>();
        private List<DATN.Shared.Employee> filter = new List<DATN.Shared.Employee>();
        private bool isLoaded = false;
        private string errorMessage;

        protected override async Task OnInitializedAsync()
        {
            await LoadEmployee();
            isLoaded = true;
        }

        private async Task LoadEmployee()
        {
            try
            {
                listEmployee = await httpClient.GetFromJsonAsync<List<DATN.Shared.Employee>>("api/Employee/GetEmployee");
                listAccount = await httpClient.GetFromJsonAsync<List<DATN.Shared.Account>>("api/Account/GetAccount");
                filter = listEmployee;
            }
            catch (Exception ex)
            {
                errorMessage = $"Error loading menu: {ex.Message}";
            }
        }

        private async Task HideEmployee(int employeeId)
        {
            try
            {
                var employee = listEmployee.FirstOrDefault(p => p.EmployeeId == employeeId);
                if (employee != null)
                {
                    await httpClient.PutAsJsonAsync($"api/Employee/{employeeId}", employee);
                    await LoadEmployee();
                    StateHasChanged();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error hiding menuitem: {ex.Message}");
            }
        }

        private async Task RestoreEmployee(int employeeId)
        {
            try
            {
                var employee = listEmployee.FirstOrDefault(p => p.EmployeeId == employeeId);
                if (employee != null)
                {
                    await httpClient.PutAsJsonAsync($"api/Employee/{employeeId}", employee);
                    await LoadEmployee();
                    StateHasChanged();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error : {ex.Message}");
            }
        }

        private void EditEmployee(int employeeId)
        {
            Navigation.NavigateTo($"/admin/editemployee/{employeeId}");
        }

        private void CreateEmployee()
        {
            Navigation.NavigateTo("/admin/createemployee");
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


