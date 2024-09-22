using DATN.Client.Service;
using DATN.Shared;
using Microsoft.JSInterop;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DATN.Client.Pages
{
    public partial class FoodOrdered
    {
        private List<Cart> carts = new List<Cart>();
        private decimal Total;
        protected override async Task OnInitializedAsync()
        {
            carts = await _localStorageService.GetCartItemAsync("historyOrder");
            if(carts.Count > 0)
            {
                CalculateTotal();
            }
        }
        private void CalculateTotal()
        {
            Total = 0;
            foreach (var item in carts)
            {
                Total += item.Price * item.Quantity;
            }
        }
        private async Task Pay()
        {
            await JS.InvokeVoidAsync("showAlert", "success", "Gọi nhân viên thành công","Bạn vui lòng đợi giây lát nhé");
        }
    }
}
