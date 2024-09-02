﻿
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
    public partial class AdminCategory
    {
        private List<DATN.Shared.Category> listCategory = new List<DATN.Shared.Category>();
        private List<DATN.Shared.Category> filteredCategory = new List<DATN.Shared.Category>();
        private bool isLoaded = false;
        private string errorMessage;

        protected override async Task OnInitializedAsync()
        {
            await LoadCategories();
            isLoaded = true;
        }

        private async Task LoadCategories()
        {
            try
            {
                listCategory = await httpClient.GetFromJsonAsync<List<DATN.Shared.Category>>("api/Category/GetCategories");
                filteredCategory = listCategory;
            }
            catch (Exception ex)
            {
                errorMessage = $"Error loading categories: {ex.Message}";
            }
        }

        private async Task DeleteCategory(int categoryId)
        {
            try
            {
                var category = listCategory.FirstOrDefault(p => p.CategoryId == categoryId);
                if (category != null)
                {
                    await httpClient.DeleteAsync($"api/Category/{categoryId}");
                    await LoadCategories();
                    StateHasChanged();
                }
            }
            catch (Exception ex)
            {
                errorMessage = $"Error deleting category: {ex.Message}";
            }
        }

        private void EditCategory(int categoryId)
        {
            Navigation.NavigateTo($"/editcategory/{categoryId}");
        }

        private void CreateCategory()
        {
            Navigation.NavigateTo($"/createcategory");
        }

        private void FilterCategories(ChangeEventArgs e)
        {
            var searchTerm = e.Value.ToString().ToLower();
            filteredCategory = string.IsNullOrWhiteSpace(searchTerm)
                ? listCategory
                : listCategory.Where(p => p.CategoryName.ToLower().Contains(searchTerm)).ToList();
        }

    }
}
