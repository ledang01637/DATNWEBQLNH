using DATN.Shared;
using Microsoft.JSInterop;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;

namespace DATN.Client.Service
{
    public class CartService
    {
        private readonly IJSRuntime _jsRuntime;
        private const string CartKey = "cart";
        public CartService(IJSRuntime jsRuntime)
        {
            _jsRuntime = jsRuntime;
        }

        public async Task AddItemToCartAsync(Cart item, int quantity)
        {
            var cart = await GetCartAsync();

            var inCart = cart.FirstOrDefault(a => a.ProductId == item.ProductId);
            if (inCart != null)
            {
                inCart.Quantity += quantity;
                await SaveCartAsync(cart);
            }
            else
            {
                item.Quantity = quantity;
                cart.Add(item);
                await SaveCartAsync(cart);
            }

            
        }
        public async Task RemoveItemFromCartAsync(Cart item)
        {
            var cart = await GetCartAsync();
            var itemToRemove = cart.Find(i => i.ProductId == item.ProductId);
            if (itemToRemove != null)
            {
                cart.Remove(itemToRemove);
            }
            await SaveCartAsync(cart);
        }
        public async Task<List<Cart>> GetCartAsync()
        {
            var cartJson = await _jsRuntime.InvokeAsync<string>("sessionStorage.getItem", CartKey);
            return string.IsNullOrEmpty(cartJson) ? new List<Cart>() : JsonSerializer.Deserialize<List<Cart>>(cartJson);
        }

        public async Task SaveCartAsync(List<Cart> cart)
        {
            var cartJson = JsonSerializer.Serialize(cart);
            await _jsRuntime.InvokeVoidAsync("sessionStorage.setItem", CartKey, cartJson);
        }

        public async Task ClearCart()
        {
            var cart = await GetCartAsync();
            cart.Clear();
        }

    }
}
