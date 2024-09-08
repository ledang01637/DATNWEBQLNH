
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
    public partial class AdminProduct
    {
        private List<DATN.Shared.Product> listProd = new List<DATN.Shared.Product>();
        private List<DATN.Shared.Unit> listUnit = new List<DATN.Shared.Unit>();
        private List<DATN.Shared.Product> filter = new List<DATN.Shared.Product>();
        private bool isLoaded = false;
        private string errorMessage;

        protected override async Task OnInitializedAsync()
        {
            await LoadProducts();
            isLoaded = true;
        }

        private async Task LoadProducts()
        {
            try
            {
                listProd = await httpClient.GetFromJsonAsync<List<DATN.Shared.Product>>("api/Product/GetProduct");
                listUnit = await httpClient.GetFromJsonAsync<List<DATN.Shared.Unit>>("api/Unit/GetUnit");
                filter = listProd;
            }
            catch (Exception ex)
            {
                errorMessage = $"Error loading product: {ex.Message}";
            }
        }

        private async Task HideProd(int productId)
        {
            try
            {
                var product = listProd.FirstOrDefault(p => p.ProductId == productId);
                if (product != null)
                {
                    product.IsDelete = true;
                    await httpClient.PutAsJsonAsync($"api/Product/{productId}", product);
                    await LoadProducts();
                    StateHasChanged();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error hiding product: {ex.Message}");
            }
        }

        private async Task RestoreProd(int productId)
        {
            try
            {
                var product = listProd.FirstOrDefault(p => p.ProductId == productId);
                if (product != null)
                {
                    product.IsDelete = false;
                    await httpClient.PutAsJsonAsync($"api/Product/{productId}", product);
                    await LoadProducts();
                    StateHasChanged();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Đã xảy ra lỗi khi khôi phục : {ex.Message}");
            }
        }

        private void EditProduct(int productId)
        {
            Navigation.NavigateTo($"/editproduct/{productId}");
        }

        private void CreateProduct()
        {
            Navigation.NavigateTo($"/createproduct");
        }

        private void Filter(ChangeEventArgs e)
        {
            var searchTerm = e.Value.ToString().ToLower();
            filter = string.IsNullOrWhiteSpace(searchTerm)
                ? listProd
                : listProd.Where(p => p.ProductName.ToLower().Contains(searchTerm)).ToList();
        }

    }
}


//private async Task DeleteCategory(int categoryId)
//{
//    try
//    {
//        var category = listCategory.FirstOrDefault(p => p.CategoryId == categoryId);
//        if (category != null)
//        {
//            await httpClient.DeleteAsync($"api/Category/{categoryId}");
//            await LoadCategories();
//            StateHasChanged();
//        }
//    }
//    catch (Exception ex)
//    {
//        errorMessage = $"Error deleting category: {ex.Message}";
//    }
//}