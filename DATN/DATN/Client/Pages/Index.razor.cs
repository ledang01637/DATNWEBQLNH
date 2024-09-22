using DATN.Client.Service;
using DATN.Shared;
using Microsoft.EntityFrameworkCore;
using Microsoft.JSInterop;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Json;
using System.Threading.Tasks;

namespace DATN.Client.Pages
{
    public partial class Index
    {
        private List<Product> products = new List<Product>();
        private List<Menu> menus = new List<Menu>();
        private List<Category> categories = new List<Category>();
        private List<MenuItem> menuItems = new List<MenuItem>();
        private string pathImg = "C:\\fakepath\\";


        protected override async Task OnInitializedAsync()
        {
            await LoadAll();
        }

        private async Task LoadAll()
        {
            try
            {
                products = await httpClient.GetFromJsonAsync<List<Product>>("api/Product/GetProduct");
                if(products != null)
                {
                    products = products.Where(a => a.IsDelete == false).ToList();
                    pathImg = pathImg.Replace("C:\\fakepath\\", "~/images/product/");
                }
                categories = await httpClient.GetFromJsonAsync<List<Category>>("api/Category/GetCategories");
                if (categories != null)
                {
                    categories = categories.Where(a => a.IsDelete == false).ToList();
                }
                menus = await httpClient.GetFromJsonAsync<List<Menu>>("api/Menu/GetMenu");
                menuItems = await httpClient.GetFromJsonAsync<List<MenuItem>>("api/MenuItem/GetMenuItem");
                if (menus.Any() && menuItems.Any())
                {
                    menus = menus.Where(a => a.IsDelete == false).ToList();
                    //menuItems = menus.Where(menuItems.Any(a => a.MenuId == a.MenuId)))
                }


            }catch(Exception ex)
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
    }
}
