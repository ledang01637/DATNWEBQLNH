using DATN.Client.Service;
using DATN.Shared;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.JSInterop;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Serialization;
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
        private Order order = new();
        private List<OrderItem> orderItems = new();
        private List<Cart> carts = new();

        private decimal Total;
        private bool isSaveOrder = false;
        private bool isUseVoucher = false;
        private int vcId;

        private HubConnection hubConnection;

        protected override async Task OnInitializedAsync()
        {
            try
            {
                hubConnection = new HubConnectionBuilder()
               .WithUrl(Navigation.ToAbsoluteUri("/ProcessHub"))
               .Build();

                await hubConnection.StartAsync();
                await LoadInit();
                //await LoadCustomerVouchers();
            }
            catch (Exception ex)
            {
                await JS.InvokeVoidAsync("showAlert", "error", ex.Message);
            }
        }

        private async Task LoadInit()
        {
            int tableId = int.Parse(await _localStorageService.GetItemAsync("tbid"));

            order = await GetOrderForTable(tableId);

            if(order == null) {
                order = new Order()
                {
                    TotalAmount = 0,
                };
                await JS.InvokeVoidAsync("showAlert", "warning", "Vui lòng đặt món ăn", ""); 

                return; 
            }

            var response = await httpClient.PostAsJsonAsync("api/OrderItem/GetOrderItemInclude", order.OrderId);

            if (response.IsSuccessStatusCode)
            {
                try
                {
                    var responseContent = await response.Content.ReadFromJsonAsync<List<OrderItem>>();

                    if (responseContent != null && responseContent.Count > 0)
                    {
                        orderItems = responseContent;
                        foreach (var item in orderItems)
                        {
                            var existingCartItem = carts.FirstOrDefault(c => c.ProductId == item.ProductId);

                            if (existingCartItem != null)
                            {
                                existingCartItem.Quantity += item.Quantity;
                            }
                            else
                            {
                                carts.Add(new Cart
                                {
                                    ProductId = item.ProductId,
                                    Price = item.Price,
                                    Quantity = item.Quantity,
                                    ProductName = item.Products.ProductName,
                                    UnitId = item.Products.UnitId,
                                    ProductImage = item.Products.ProductImage
                                });
                            }
                        }
                    }

                    else
                    {
                        await JS.InvokeVoidAsync("showAlert", "error", "Lỗi", "Không tìm thấy bàn");
                    }
                }
                catch(Exception ex)
                {
                    await JS.InvokeVoidAsync("showAlert", "error", "Lỗi", "Liên hệ Admin: " + ex.Message);
                }
                
            }
            else
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                await JS.InvokeVoidAsync("showAlert", "error", "Lỗi", $"Lỗi khi gọi API: {response.StatusCode} - Nội dung: {errorContent}");
            }
        }

        private async Task<Order> GetOrderForTable(int tableId)
        {
            var response = await httpClient.PostAsJsonAsync("api/Order/GetOrderStatus", tableId);

            if (response.IsSuccessStatusCode)
            {
                var responseContent = await response.Content.ReadFromJsonAsync<Order>();

                if (responseContent != null && responseContent.Status.Equals("Đang xử lý"))
                {
                    return responseContent;
                }
                return null;
            }
            else
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                await JS.InvokeVoidAsync("showAlert", "error", "Lỗi", $"Lỗi khi gọi API: {response.StatusCode} - Nội dung: {errorContent}");
            }

            return null;
        }

        private async Task Payment()
        {
            try
            {
                if (hubConnection is not null && hubConnection.State == HubConnectionState.Connected)
                {
                    string token = await _localStorageService.GetItemAsync("n");

                    if(string.IsNullOrEmpty(token)) { await JS.InvokeVoidAsync("showAlert", "error", "Vui lòng quét mã QR"); return ; }

                    int numberTable = GetTableNumberFromToken(token);
                   
                    string accountType = await CheckTypeAccount();

                    await hubConnection.SendAsync("SendMessage", "paymentReq");
                    await JS.InvokeVoidAsync("showAlert", "success", "Gọi nhân viên thành công", "Bạn vui lòng đợi giây lát nhé");
                }
                else
                {
                    await JS.InvokeVoidAsync("alert", "Không thể kết nối tới server!");
                }
            }
            catch (Exception ex)
            {
                await JS.InvokeVoidAsync("showAlert", "error", "Lỗi thanh toán", ex.Message);
            }
        }

        //private async Task<Customer> GetCustomerNoAccount()
        //{
        //    customers = await httpClient.GetFromJsonAsync<List<Customer>>("api/Customer/GetCustomerInclude");
        //    return customers.FirstOrDefault(a => a.Accounts.AccountType.ToLower().Equals("no account"));
        //}
        private int GetTableNumberFromToken(string token)
        {
            var handler = new JwtSecurityTokenHandler();
            var jwtToken = handler.ReadToken(token) as JwtSecurityToken;
            var userId = jwtToken?.Claims.FirstOrDefault(c => c.Type == "userId");
            return int.Parse(userId?.Value);
        }
        //private async Task SaveRewarPointes(Order _order)
        //{
        //    try
        //    {
        //        customers = await httpClient.GetFromJsonAsync<List<Customer>>("api/Customer/GetCustomer");
        //        rewardPointes = await httpClient.GetFromJsonAsync<List<RewardPointe>>("api/RewardPointe/GetRewardPointe");

        //        if (customers != null)
        //        {
        //            var accountId = await CheckAccountId();
        //            var customer = customers.FirstOrDefault(c => c.AccountId == accountId);

        //            if (customer != null)
        //            {
        //                int newRewardPoints = (int)(_order.TotalAmount / 100000);

        //                var existingRewardPoints = rewardPointes.FirstOrDefault(rp => rp.CustomerId == customer.CustomerId);

        //                if (existingRewardPoints != null)
        //                {
        //                    existingRewardPoints.RewardPoint += newRewardPoints;
        //                    existingRewardPoints.OrderId = _order.OrderId;
        //                    existingRewardPoints.UpdateDate = DateTime.Now;

        //                    var res = await httpClient.PutAsJsonAsync($"api/RewardPointe/{existingRewardPoints.RewardPointId}", existingRewardPoints);
        //                    if (!res.IsSuccessStatusCode)
        //                    {
        //                        await JS.InvokeVoidAsync("showAlert", "error", "Lỗi", "Không thể cập nhật điểm thưởng");
        //                    }
        //                }
        //                else
        //                {
        //                    rewardPointe = new RewardPointe()
        //                    {
        //                        CustomerId = customer.CustomerId,
        //                        RewardPoint = newRewardPoints,
        //                        UpdateDate = DateTime.Now,
        //                        IsDeleted = false,
        //                        OrderId = _order.OrderId,
        //                    };

        //                    var response = await httpClient.PostAsJsonAsync("api/RewardPointe/AddRewardPointe", rewardPointe);
        //                    if (response.IsSuccessStatusCode)
        //                    {
        //                        var createdRewardPoint = await response.Content.ReadFromJsonAsync<RewardPointe>();
        //                        customer.TotalRewardPoint = createdRewardPoint.RewardPoint;
        //                        await httpClient.PutAsJsonAsync($"api/Customer/{customer.CustomerId}", customer);
        //                    }
        //                    else
        //                    {
        //                        await JS.InvokeVoidAsync("showAlert", "error", "Lỗi", "Điểm chưa được thêm");
        //                    }
        //                }
        //            }
        //            else
        //            {
        //                await JS.InvokeVoidAsync("showAlert", "error", "Lỗi", "Khách hàng không tìm thấy");
        //            }
        //        }
        //        else
        //        {
        //            await JS.InvokeVoidAsync("showAlert", "error", "Lỗi", "Khách hàng không tìm thấy");
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        await JS.InvokeVoidAsync("showAlert", "error", "Lỗi", ex.Message);
        //    }
        //}
        private async Task<string> CheckTypeAccount()
        {
            var token = await _localStorageService.GetItemAsync("authToken");
            if (!String.IsNullOrEmpty(token))
            {
                var handler = new JwtSecurityTokenHandler();
                var jwtToken = handler.ReadToken(token) as JwtSecurityToken;
                return jwtToken.Claims.FirstOrDefault(c => c.Type == "AccountType")?.Value;
            }
            return null;
        }

        public async ValueTask DisposeAsync()
        {
            if (hubConnection is not null)
            {
                await hubConnection.DisposeAsync();
            }
        }
    }
}
