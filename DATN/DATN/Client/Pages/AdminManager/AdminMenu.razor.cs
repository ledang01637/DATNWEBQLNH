
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
        private List<DATN.Shared.MenuItem> listMenuItem = new List<DATN.Shared.MenuItem>();
        private List<DATN.Shared.Product> listProduct = new List<DATN.Shared.Product>();
        private List<DATN.Shared.MenuItem> filtermenuitem = new List<DATN.Shared.MenuItem>();
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
                listMenuItem = await httpClient.GetFromJsonAsync<List<DATN.Shared.MenuItem>>("api/MenuItem/GetMenuItem");
                listMenu = await httpClient.GetFromJsonAsync<List<DATN.Shared.Menu>>("api/Menu/GetMenu");
                listProduct = await httpClient.GetFromJsonAsync<List<DATN.Shared.Product>>("api/Product/GetProduct");
                filter = listMenu;
                filtermenuitem = listMenuItem;
            }
            catch (Exception ex)
            {
                errorMessage = $"Error loading menu: {ex.Message}";
            }
        }

        //Menu
        private async Task HideMenu(int menuId)
        {
            try
            {
                var menu = listMenu.FirstOrDefault(p => p.MenuId == menuId);
                if (menu != null)
                {
                    menu.IsDeleted = true;
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

        private async Task RestoreMenu(int menuId)
        {
            try
            {
                var menu = listMenu.FirstOrDefault(p => p.MenuId == menuId);
                if (menu != null)
                {
                    menu.IsDeleted = false;
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
            Navigation.NavigateTo($"/admin/editmenu/{menuId}");
        }

        private void CreateMenu()
        {
            Navigation.NavigateTo($"/admin/createmenu");
        }

        private void Filter(ChangeEventArgs e)
        {
            var searchTerm = e.Value.ToString().ToLower();
            filter = string.IsNullOrWhiteSpace(searchTerm)
                ? listMenu
                : listMenu.Where(p => p.MenuName.ToLower().Contains(searchTerm)).ToList();
        }


        //MenuItem
        private async Task HideMenuItem(int menuitemId)
        {
            try
            {
                var menuitem = listMenuItem.FirstOrDefault(p => p.MenuItemId == menuitemId);
                if (menuitem != null)
                {
                    menuitem.IsDeleted = true;
                    await httpClient.PutAsJsonAsync($"api/MenuItem/{menuitemId}", menuitem);
                    await LoadMenus();
                    StateHasChanged();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error hiding menuitem: {ex.Message}");
            }
        }

        private async Task RestoreMenuItem(int menuitemId)
        {
            try
            {
                var menuitem = listMenuItem.FirstOrDefault(p => p.MenuItemId == menuitemId);
                if (menuitem != null)
                {
                    menuitem.IsDeleted = false;
                    await httpClient.PutAsJsonAsync($"api/MenuItem/{menuitemId}", menuitem);
                    await LoadMenus();
                    StateHasChanged();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error : {ex.Message}");
            }
        }

        private void EditMenuItem(int menuitemId)
        {
            Navigation.NavigateTo($"/editmenuitem/{menuitemId}");
        }

        private void CreateMenuItem()
        {
            Navigation.NavigateTo($"/createmenuitem");
        }

        private void FilterMenuItem(ChangeEventArgs e)
        {
            var searchTermmenuitem = e.Value.ToString().ToLower();
            filtermenuitem = string.IsNullOrWhiteSpace(searchTermmenuitem)
                ? listMenuItem
                : listMenuItem.Where(p => p.MenuItemId.Equals(searchTermmenuitem)).ToList();
        }



    }
}


