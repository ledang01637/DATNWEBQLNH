using DATN.Client.Service;
using DATN.Shared;
using Microsoft.AspNetCore.SignalR.Client;
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
        private List<Table> tables = new List<Table>(); 
        private List<Voucher> vouchers = new List<Voucher>(); 
        private Order order = new Order();
        private OrderItem orderItem = new OrderItem();
        private List<RewardPointe> rewardPointes = new List<RewardPointe>();
        private RewardPointe rewardPointe = new RewardPointe();

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

                carts = await _localStorageService.GetCartItemAsync("historyOrder");
                if (carts.Any())
                {
                    CalculateTotal();
                }
                await hubConnection.StartAsync();
                await LoadCustomerVouchers();
            }
            catch (Exception ex)
            {
                await JS.InvokeVoidAsync("showAlert", "error", ex.Message);
            }
        }

        private void CalculateTotal()
        {
            Total = carts.Sum(item => item.Price * item.Quantity);
        }

        private async Task LoadCustomerVouchers()
        {
            try
            {
                customerVouchers = await httpClient.GetFromJsonAsync<List<CustomerVoucher>>("api/CustomerVoucher/GetCustomerVoucher");
            }
            catch (Exception ex)
            {
                await JS.InvokeVoidAsync("showAlert", "error", "Lỗi tải voucher", ex.Message);
            }
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

                    await LoadTables();
                    Table table = tables.FirstOrDefault(a => a.TableNumber == numberTable);

                    if (table == null) throw new Exception("Không tìm thấy bàn!");

                    string accountType = await CheckTypeAccount();

                    if (accountType == "No Account")
                    {
                        await HandleNoAccountPayment(table);
                    }
                    else
                    {
                        await HandleAccountPayment(table);
                    }
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

        private async Task HandleNoAccountPayment(Table table)
        {
            Customer customerNoAccount = await GetCustomerNoAccount();
            order = new Order
            {
                TableId = table.TableId,
                CreateDate = DateTime.Now,
                TotalAmount = Total,
                Status = "Đang xử lý",
                CustomerId = customerNoAccount.CustomerId,
                PaymentMethod = "",
                CustomerVoucherId = 0
            };

            await ProcessOrder();
        }

        private async Task HandleAccountPayment(Table table)
        {
            Customer customerNoAccount = await GetCustomerNoAccount();
            order = new Order
            {
                TableId = table.TableNumber,
                CreateDate = DateTime.Now,
                TotalAmount = Total,
                Status = "Đang xử lý",
                CustomerId = customerNoAccount.CustomerId,
                PaymentMethod = "",
                CustomerVoucherId = isUseVoucher ? vcId : 0
            };

            await ProcessOrder();
        }

        private async Task ProcessOrder()
        {
            await SaveOrder(order);

            if (isSaveOrder && carts != null)
            {
                foreach (var item in carts)
                {
                    await SaveOrderItem(new OrderItem
                    {
                        ProductId = item.ProductId,
                        OrderId = order.OrderId,
                        Quantity = item.Quantity,
                        Price = item.Price,
                        TotalPrice = item.Quantity * item.Price
                    });
                }

                await SaveRewarPointes(order);
                await _cartService.ClearCart();
                Navigation.NavigateTo("/");
            }
        }

        private async Task LoadTables()
        {
            tables = await httpClient.GetFromJsonAsync<List<Table>>("api/Table/GetTable");
        }

        private async Task<Customer> GetCustomerNoAccount()
        {
            customers = await httpClient.GetFromJsonAsync<List<Customer>>("api/Customer/GetCustomer");
            return customers.FirstOrDefault(a => a.AccountId == 2);
        }

        private int GetTableNumberFromToken(string token)
        {
            var handler = new JwtSecurityTokenHandler();
            var jwtToken = handler.ReadToken(token) as JwtSecurityToken;
            var userId = jwtToken?.Claims.FirstOrDefault(c => c.Type == "userId");
            return int.Parse(userId?.Value);
        }

        private async Task SaveOrder(Order _order)
        {
            var response = await httpClient.PostAsJsonAsync("api/Order/AddOrder", _order);

            if (response.IsSuccessStatusCode)
            {
                _order.OrderId = (await response.Content.ReadFromJsonAsync<Order>()).OrderId;
                isSaveOrder = true;
            }
            else
            {
                throw new Exception("Lỗi khi thêm đơn hàng");
            }
        }

        private async Task SaveOrderItem(OrderItem _orderItem)
        {
            try
            {
                await httpClient.PostAsJsonAsync("api/OrderItem/AddOrderItem", _orderItem);
            }
            catch (Exception ex)
            {
                throw new Exception("Lỗi khi thêm sản phẩm vào đơn hàng: " + ex.Message);
            }
        }

        private async Task SaveRewarPointes(Order _order)
        {
            try
            {
                customers = await httpClient.GetFromJsonAsync<List<Customer>>("api/Customer/GetCustomer");
                rewardPointes = await httpClient.GetFromJsonAsync<List<RewardPointe>>("api/RewardPointe/GetRewardPointe");

                if (customers != null)
                {
                    var accountId = await _localStorageService.GetItemAsync("AccountId");
                    var customer = customers.FirstOrDefault(c => c.AccountId == int.Parse(accountId));

                    if (customer != null)
                    {
                        int newRewardPoints = (int)(_order.TotalAmount / 100000);

                        var existingRewardPoints = rewardPointes.FirstOrDefault(rp => rp.CustomerId == customer.CustomerId);

                        if (existingRewardPoints != null)
                        {
                            existingRewardPoints.RewardPoint += newRewardPoints;
                            existingRewardPoints.OrderId = _order.OrderId;
                            existingRewardPoints.UpdateDate = DateTime.Now;

                            var res = await httpClient.PutAsJsonAsync($"api/RewardPointe/{existingRewardPoints.RewardPointId}", existingRewardPoints);
                            if (res.IsSuccessStatusCode)
                            {
                                await JS.InvokeVoidAsync("showAlert", "success", "Cộng điểm thành công", "Đã cộng: " + newRewardPoints);
                            }
                            else
                            {
                                await JS.InvokeVoidAsync("showAlert", "error", "Lỗi", "Không thể cập nhật điểm thưởng");
                            }
                        }
                        else
                        {
                            rewardPointe = new RewardPointe()
                            {
                                CustomerId = customer.CustomerId,
                                RewardPoint = newRewardPoints,
                                UpdateDate = DateTime.Now,
                                IsDeleted = false,
                                OrderId = _order.OrderId,
                            };

                            var response = await httpClient.PostAsJsonAsync("api/RewardPointe/AddRewardPointe", rewardPointe);
                            if (response.IsSuccessStatusCode)
                            {
                                var createdRewardPoint = await response.Content.ReadFromJsonAsync<RewardPointe>();
                                customer.TotalRewardPoint = createdRewardPoint.RewardPoint;
                                await httpClient.PutAsJsonAsync($"api/Customer/{customer.CustomerId}", customer);
                            }
                            else
                            {
                                await JS.InvokeVoidAsync("showAlert", "error", "Lỗi", "Điểm chưa được thêm");
                            }
                        }
                    }
                    else
                    {
                        await JS.InvokeVoidAsync("showAlert", "error", "Lỗi", "Khách hàng không tìm thấy");
                    }
                }
                else
                {
                    await JS.InvokeVoidAsync("showAlert", "error", "Lỗi", "Khách hàng không tìm thấy");
                }
            }
            catch (Exception ex)
            {
                await JS.InvokeVoidAsync("showAlert", "error", "Lỗi", ex.Message);
            }
        }


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
