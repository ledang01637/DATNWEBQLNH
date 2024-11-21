
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
    public partial class AdminMenu
    {
        private List<DATN.Shared.Menu> listMenu = new List<DATN.Shared.Menu>();
        private List<DATN.Shared.Menu> filter = new List<DATN.Shared.Menu>();
        private bool isLoaded = false;
        private string errorMessage;

        protected override async Task OnInitializedAsync()
        {
            await LoadMenus();
            isLoaded = true;
        }

        private async Task LoadMenus()
        {
            try
            {
                listMenu = await httpClient.GetFromJsonAsync<List<DATN.Shared.Menu>>("api/Menu/GetMenu");
                filter = listMenu;
            }
            catch (Exception ex)
            {
                errorMessage = $"Error loading menu: {ex.Message}";
            }
        }

        private async Task Hide(int menuId)
        {
            try
            {
                var menu = listMenu.FirstOrDefault(p => p.MenuId == menuId);
                if (menu != null)
                {
                    menu.IsDelete = true;
                    await httpClient.PutAsJsonAsync($"api/Menu/{menuId}", menu);
                    await LoadMenus();
                    StateHasChanged();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error hiding menuitem: {ex.Message}");
            }
        }

        private async Task Restore(int menuId)
        {
            try
            {
                var menu = listMenu.FirstOrDefault(p => p.MenuId == menuId);
                if (menu != null)
                {
                    menu.IsDelete = false;
                    await httpClient.PutAsJsonAsync($"api/Menu/{menuId}", menu);
                    await LoadMenus();
                    StateHasChanged();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error : {ex.Message}");
            }
        }

        private void EditMenu(int menuId)
        {
            Navigation.NavigateTo($"/editmenu/{menuId}");
        }

        private void CreateMenu()
        {
            Navigation.NavigateTo($"/createmenu");
        }

        private void Filter(ChangeEventArgs e)
        {
            var searchTerm = e.Value.ToString().ToLower();
            filter = string.IsNullOrWhiteSpace(searchTerm)
                ? listMenu
                : listMenu.Where(p => p.MenuName.ToLower().Contains(searchTerm)).ToList();
        }



    }
}


