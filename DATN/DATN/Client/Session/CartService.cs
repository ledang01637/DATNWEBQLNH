using DATN.Shared;
using Microsoft.JSInterop;
using System.Collections.Generic;
using System.Text.Json;
using System.Threading.Tasks;

namespace DATN.Server.Service
{
    public class CartService
    {
        private readonly IJSRuntime _jsRuntime;
        private const string CartKey = "cart";

        public CartService(IJSRuntime jsRuntime)
        {
            _jsRuntime = jsRuntime;
        }

        public async Task AddItemToCartAsync(Product item)
        {
            var cart = await GetCartAsync();
            cart.Add(item);
            await SaveCartAsync(cart);
        }
        public async Task RemoveItemFromCartAsync(Product item)
        {
            var cart = await GetCartAsync();
            var itemToRemove = cart.Find(i => i.ProductId == item.ProductId);
            if (itemToRemove != null)
            {
                cart.Remove(itemToRemove);
            }
            await SaveCartAsync(cart);
        }
        public async Task<List<Product>> GetCartAsync()
        {
            var cartJson = await _jsRuntime.InvokeAsync<string>("sessionStorage.getItem", CartKey);
            return string.IsNullOrEmpty(cartJson) ? new List<Product>() : JsonSerializer.Deserialize<List<Product>>(cartJson);
        }

        public async Task SaveCartAsync(List<Product> cart)
        {
            var cartJson = JsonSerializer.Serialize(cart);
            await _jsRuntime.InvokeVoidAsync("sessionStorage.setItem", CartKey, cartJson);
        }
    }

}
