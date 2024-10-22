using DATN.Client.Service;
using DATN.Shared;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.JSInterop;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DATN.Client.Pages
{
    public partial class OrderList
    {
        private List<Cart> cartItems = new List<Cart>();
        private decimal Total;

        protected override async Task OnInitializedAsync()
        {
            cartItems = await _cartService.GetCartAsync();
            CalculateTotal();
        }

        private async Task RemoveFromCart(Cart product)
        {
            if (product != null)
            {
                if (product.Quantity > 1)
                {
                    product.Quantity -= 1;
                    await _cartService.SaveCartAsync(cartItems);
                }
                else
                {
                    await _cartService.RemoveItemFromCartAsync(product);
                }
                cartItems = await _cartService.GetCartAsync();
                CalculateTotal();
            }
        }

        private void CalculateTotal()
        {
            Total = 0;
            foreach (var item in cartItems)
            {
                Total += item.Price * item.Quantity;
            }
        }
        private async Task Order()
        {
            if (cartItems.Count() > 0)
            {
                var expiryTime = DateTime.Now.AddMinutes(30).ToString("o");
                await _localStorageService.SetCartItemAsync("historyOrder", cartItems);
                await _localStorageService.SetItemAsync("cartExpiryTime", expiryTime);
                await JS.InvokeVoidAsync("showAlert", "success", "Đặt món thành công", "Bạn vui lòng đợi đầu bếp làm nha :3");
                await Task.Delay(1000);
                Navigation.NavigateTo("/food-ordered");
            }
            else
            {
                await JS.InvokeVoidAsync("showAlert", "warning", "Vui lòng thêm món ăn", "");
            }
        }
    }
}
