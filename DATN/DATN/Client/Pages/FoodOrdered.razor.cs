using DATN.Client.Service;
using DATN.Shared;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DATN.Client.Pages
{
    public partial class FoodOrdered
    {
        private List<Cart> carts = new List<Cart>();
        protected override async Task OnInitializedAsync()
        {
            carts = await _localStorageService.GetCartItemAsync("historyOrder");
        }
    }
}
