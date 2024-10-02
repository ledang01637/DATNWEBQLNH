using DATN.Client.Service;
using DATN.Shared;
using Microsoft.JSInterop;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Net.Http.Json;
using System.Threading.Tasks;

namespace DATN.Client.Pages
{
    public partial class FoodOrdered
    {
        private List<Cart> carts = new List<Cart>();
        private List<Customer> customers = new List<Customer>();
        private List<CustomerVoucher> customerVouchers = new List<CustomerVoucher>(); 
        private Order order = new Order();
        private OrderItem orderItem = new OrderItem();
        private RewardPointe rewardPointe = new RewardPointe();

        private decimal Total;
        private bool isSaveOrder = false;
        protected override async Task OnInitializedAsync()
        {
            carts = await _localStorageService.GetCartItemAsync("historyOrder");
            if(carts.Count > 0)
            {
                CalculateTotal();
            }
            customerVouchers = await httpClient.GetFromJsonAsync<List<CustomerVoucher>>("api/CustomerVoucher/GetCustomerVoucher");
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
                OrderDate = DateTime.Now,
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
                await SaveRewarPointes(order);
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

        private async Task SaveRewarPointes(Order _order)
        {
            customers = await httpClient.GetFromJsonAsync<List<Customer>>("api/Customer/GetCustomer");

            if(customers != null)
            {
                var accountId = await _localStorageService.GetItemAsync("AccountId");
                var customer = customers.FirstOrDefault(c => c.AccountId == int.Parse(accountId));

                if (customer != null)
                {
                    rewardPointe = new RewardPointe()
                    {
                        CustomerId = customer.CustomerId,
                        RewardPoint = int.Parse(_order.TotalAmount.ToString()),
                        UpdateDate = DateTime.Now,
                        IsDeleted = false,
                        OrderId = _order.OrderId,
                    };
                    var response = await httpClient.PostAsJsonAsync("api/RewardPointe/AddRewardPointe", rewardPointe);
                    if (response.IsSuccessStatusCode)
                    {
                        var createdrewardPointe = await response.Content.ReadFromJsonAsync<RewardPointe>();
                        customer.TotalRewardPoint = createdrewardPointe.RewardPoint;
                        await httpClient.PutAsJsonAsync($"api/Customer/{customer.CustomerId}", customer);
                    }
                }
                else
                {
                    Console.WriteLine("customer is null");
                }
            }
            else
            {
                Console.WriteLine("customers is null");
            }

        }

        private async Task UseVoucher(int voucherId)
        {
            var vc = await httpClient.GetFromJsonAsync<Voucher>($"api/Voucher/{voucherId}");
            Total = Total * vc.DiscountValue;
            StateHasChanged();
        }

        private async Task<string> CheckTypeAccount()
        {
            var token = await _localStorageService.GetItemAsync("authToken");
            if(!String.IsNullOrEmpty(token))
            {
                var handler = new JwtSecurityTokenHandler();
                var jwtToken = handler.ReadToken(token) as JwtSecurityToken;
                var AccountType = jwtToken.Claims.FirstOrDefault(c => c.Type == "AccountType")?.Value;

                return AccountType;
            }
            else
            {
                return null;
            }
            
        }
    }
}
