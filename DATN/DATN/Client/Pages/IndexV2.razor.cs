﻿using DATN.Shared;
using Microsoft.JSInterop;
using System.Collections.Generic;
using System.Net.Http;
using System;
using System.Threading.Tasks;
using System.Net.Http.Json;
using System.Linq;
using Microsoft.AspNetCore.Components;

namespace DATN.Client.Pages
{
    public partial class IndexV2
    {
        private List<Product> products = new List<Product>();
        private List<Menu> menus = new List<Menu>();
        private List<Category> categories = new List<Category>();
        private List<MenuItem> menuItems = new List<MenuItem>();
        private string ComboName;

        protected override async Task OnInitializedAsync()
        {
            await LoadAll();
            await JS.InvokeVoidAsync("initializeIsotope");
        }
        private async Task LoadAll()
        {
            try
            {
                products = await httpClient.GetFromJsonAsync<List<Product>>("api/Product/GetProduct");
                if (products != null)
                {
                    products = products.Where(a => a.IsDelete == false).ToList();
                }
                categories = await httpClient.GetFromJsonAsync<List<Category>>("api/Category/GetCategories");
                if (categories != null)
                {
                    categories = categories.Where(a => a.IsDelete == false).ToList();
                }
                menus = await httpClient.GetFromJsonAsync<List<Menu>>("api/Menu/GetMenu");

                StateHasChanged();

            }
            catch (Exception ex)
            {
                var query = $"[C#] fix error bằng tiếng việt: {ex.Message}";
                await JS.InvokeVoidAsync("openChatGPT", query);
            }
        }
   
        
        private async Task AddToCart(Product product)
        {
            Cart cart = new Cart();
            cart.ProductId = product.ProductId;
            cart.ProductName = product.ProductName;
            cart.Price = product.Price;
            cart.ProductImage = product.ProductImage;

            await _cartService.AddItemToCartAsync(cart, 1);
            Navigation.NavigateTo("/order-list");
        }
        private async Task LoadCombo(int MenuId)
        {
            if (MenuId > 0)
            {
                var a = menus.FirstOrDefault(a => a.MenuId == MenuId);
                ComboName = a?.MenuName;

                menuItems = await httpClient.GetFromJsonAsync<List<MenuItem>>("api/MenuItem/GetMenuItem");
                menuItems = menuItems.Where(a => a.MenuId == MenuId).ToList();

                var productIds = menuItems.Select(a => a.ProductId).ToList();

                await LoadAll();

                products = products.Where(p => productIds.Contains(p.ProductId)).ToList();

                StateHasChanged();
            }
        }
        private async Task FilterProducts(ChangeEventArgs e)
        {
            var searchTerm = e.Value.ToString().ToLower();
            await LoadAll();
            if (products != null)
            {
                products = products.Where(p => p.ProductName.ToLower().Contains(searchTerm)).ToList();
            }
        }
    }
}