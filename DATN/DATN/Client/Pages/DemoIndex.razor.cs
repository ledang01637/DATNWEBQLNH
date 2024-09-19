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

        private string token;
        private Table table = new Table();
        private List<Table> tables = new List<Table>();
        private List<Product> products = new List<Product>();

        protected override async Task OnInitializedAsync()
        {
            var uri = Navigation.ToAbsoluteUri(Navigation.Uri);
            if (QueryHelpers.ParseQuery(uri.Query).TryGetValue("token", out var extractedToken))
            {
                token = extractedToken;
                var decodedToken = Uri.UnescapeDataString(token);
                var handler = new JwtSecurityTokenHandler();
                var jsonToken = handler.ReadToken(decodedToken) as JwtSecurityToken;
                tables = await httpClient.GetFromJsonAsync<List<Table>>("api/Table/GetTable");
                var tableNumber = jsonToken.Claims.FirstOrDefault(c => c.Type == "tableNumber")?.Value;
                table = tables.FirstOrDefault(a => a.TableNumber == int.Parse(tableNumber));
                products = await httpClient.GetFromJsonAsync<List<Product>>("api/Product/GetProduct");
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
    }
}
