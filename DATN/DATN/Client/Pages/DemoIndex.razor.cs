using DATN.Shared;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using Newtonsoft.Json.Linq;
using System;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.WebUtilities;
using System.Collections.Generic;
using DATN.Client.Service;

namespace DATN.Client.Pages
{
    public partial class DemoIndex
    {
        private Table table = new Table();
        private List<Table> tables = new List<Table>();
        private List<Product> products = new List<Product>();

        protected override async Task OnInitializedAsync()
        {
            var uri = Navigation.ToAbsoluteUri(Navigation.Uri);
            if (QueryHelpers.ParseQuery(uri.Query).TryGetValue("n", out var extractedMd5))
            {
                await ProcessMd5Value(extractedMd5);
            }
        }
        private async Task AddToCart(Product product)
        {
            Cart cart = new Cart();
            cart.ProductId = product.ProductId;
            cart.ProductName = product.ProductName;
            cart.Price = product.Price;
            cart.ProductImage = product.ProductImage;

            await _cartService.AddItemToCartAsync(cart,1);
            Navigation.NavigateTo("/order-list");
        }
        private async Task ProcessMd5Value(string md5)
        {
            tables = await httpClient.GetFromJsonAsync<List<Table>>("api/Table/GetTable");

            foreach (var t in tables)
            {
                var encodedTableNumber = await GenerateMD5Hash(t.TableNumber.ToString());
                if (encodedTableNumber == md5)
                {
                    table = t;
                    break;
                }
            }

            if (table != null)
            {
                products = await httpClient.GetFromJsonAsync<List<Product>>("api/Product/GetProduct");
            }
        }

        private async Task<string> GenerateMD5Hash(string input)
        {
            return await JS.InvokeAsync<string>("generateMD5Hash", input);
        }

    }
}
