using DATN.Client.Service;
using DATN.Shared;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.JSInterop;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DATN.Client.Pages
{
    public partial class OrderList
    {
        private List<Cart> cartItems = new List<Cart>();
        private decimal Total;
        private decimal Fee = 0;
        private decimal Pay;

        protected override async Task OnInitializedAsync()
        {
            cartItems = await _cartService.GetCartAsync();
        }

        private async Task RemoveFromCart(Cart product)
        {
            if (product != null)
            {
                await _cartService.RemoveItemFromCartAsync(product);
                cartItems = await _cartService.GetCartAsync();
                CalculateTotal();
            }
        }
        private async Task ThanhtoanAsync()
        {
            if (cartItems.Count() > 0)
            {
                Navigation.NavigateTo("/payment");
            }
            else
            {
                await JS.InvokeVoidAsync("showAlert", "EmptyPro");
            }
        }
        private void CalculateTotal()
        {
            Total = 0;
            foreach (var item in cartItems)
            {
                Total += item.Price;
            }
            Pay = Total + Fee;
        }
    }
}
