
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
    public partial class AdminCategory
    {
        private List<Category> listCategory = new List<Category>();
        private List<Category> filteredCategory = new List<Category>();
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
                listCategory = await httpClient.GetFromJsonAsync<List<Category>>("api/Category/GetCategories");
                filteredCategory = listCategory;
            }
            catch (Exception ex)
            {
                errorMessage = $"Error loading categories: {ex.Message}";
            }
        }

        private async Task HideProd(int categoryId)
        {
            try
            {
                var category = listCategory.FirstOrDefault(p => p.CategoryId == categoryId);
                if (category != null)
                {
                    category.IsDeleted = true;
                    await httpClient.PutAsJsonAsync($"api/Category/EditCategory/{categoryId}", category);
                    await LoadCategories();
                    StateHasChanged();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error hiding caterory: {ex.Message}");
            }
        }

        private async Task RestoreProd(int categoryId)
        {
            try
            {
                var category = listCategory.FirstOrDefault(p => p.CategoryId == categoryId);
                if (category != null)
                {
                    category.IsDeleted = false;
                    await httpClient.PutAsJsonAsync($"api/Category/EditCategory/{categoryId}", category);
                    await LoadCategories();
                    StateHasChanged();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Đã xảy ra lỗi khi khôi phục : {ex.Message}");
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

    }
}

