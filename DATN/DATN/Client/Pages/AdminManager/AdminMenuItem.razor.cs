
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
    public partial class AdminMenuItem
    {
        private List<DATN.Shared.MenuItem> listMenuItem = new List<DATN.Shared.MenuItem>();
        private List<DATN.Shared.Menu> listMenu = new List<DATN.Shared.Menu>();
        private List<DATN.Shared.Product> listProduct = new List<DATN.Shared.Product>();
        private List<DATN.Shared.MenuItem> filter = new List<DATN.Shared.MenuItem>();
        private bool isLoaded = false;
        private string errorMessage;

        protected override async Task OnInitializedAsync()
        {
            await LoadMenuItems();
            isLoaded = true;
        }

        private async Task LoadMenuItems()
        {
            try
            {
                listMenuItem = await httpClient.GetFromJsonAsync<List<DATN.Shared.MenuItem>>("api/MenuItem/GetMenuItem");
                listMenu = await httpClient.GetFromJsonAsync<List<DATN.Shared.Menu>>("api/Menu/GetMenu");
                listProduct = await httpClient.GetFromJsonAsync<List<DATN.Shared.Product>>("api/Product/GetProduct");
                filter = listMenuItem;
            }
            catch (Exception ex)
            {
                errorMessage = $"Error loading menuitem: {ex.Message}";
            }
        }

        private async Task Hide(int menuitemId)
        {
            try
            {
                var menuitem = listMenuItem.FirstOrDefault(p => p.MenuItemId == menuitemId);
                if (menuitem != null)
                {
                    menuitem.IsDeleted = true;
                    await httpClient.PutAsJsonAsync($"api/MenuItem/{menuitemId}", menuitem);
                    await LoadMenuItems();
                    StateHasChanged();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error hiding menuitem: {ex.Message}");
            }
        }

        private async Task Restore(int menuitemId)
        {
            try
            {
                var menuitem = listMenuItem.FirstOrDefault(p => p.MenuItemId == menuitemId);
                if (menuitem != null)
                {
                    menuitem.IsDeleted = false;
                    await httpClient.PutAsJsonAsync($"api/MenuItem/{menuitemId}", menuitem);
                    await LoadMenuItems();
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

        //private void Filter(ChangeEventArgs e)
        //{
        //    var searchTerm = e.Value.ToString().ToLower();
        //    filter = string.IsNullOrWhiteSpace(searchTerm)
        //        ? listMenuItem
        //        : listMenuItem.Where(p => p.MenuItemId.ToLower().Contains(searchTerm)).ToList();
        //}

    }
}


