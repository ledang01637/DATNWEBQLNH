using DATN.Client.Service;
using DATN.Shared;
using Microsoft.JSInterop;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.Net.Http.Json;
using System.Threading.Tasks;

namespace DATN.Client.Pages
{
    public partial class FoodOrdered
    {
        private List<Cart> carts = new List<Cart>();
        private Order order = new Order();
        private OrderItem orderItem = new OrderItem();
        private decimal Total;
        private bool isSaveOrder = false;
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
            var numberTable = await _localStorageService.GetItemAsync("n");

            order = new Order()
            {
                TableId = int.Parse(numberTable),
                OrderDate = System.DateTime.Now,
                TotalAmount = Total,
                Status = "Đang xử lý",
                CustomerId = 1,
                PaymentMethod = "",
                CustomerVoucherId = null
            };

            await SaveOrder(order);

            if (carts != null && isSaveOrder)
            {
                foreach (var item in carts)
                {
                    orderItem = new OrderItem()
                    {
                        ProductId = item.ProductId,
                        OrderId = order.OrderId,
                        Quantity = item.Quantity,
                        Price = item.Price,
                        TotalPrice = item.Quantity * item.Price,
                    };
                    await SaveOrderItem(orderItem);
                }
            }

            await JS.InvokeVoidAsync("showAlert", "success", "Gọi nhân viên thành công","Bạn vui lòng đợi giây lát nhé");
        }

        private async Task SaveOrder(Order _order)
        {
            var response = await httpClient.PostAsJsonAsync("api/Order/AddOrder", _order);

            if (response.IsSuccessStatusCode)
            {
                var createdOrder = await response.Content.ReadFromJsonAsync<Order>();
                _order.OrderId = createdOrder.OrderId;
                isSaveOrder = true;
            }
        }
        private async Task SaveOrderItem(OrderItem _orderItem)
        {
            await httpClient.PostAsJsonAsync("api/OrderItem/AddOrderItem", _orderItem);
        }

    }
}
