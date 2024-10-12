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
        protected override async Task OnInitializedAsync()
        {
            try
            {
                carts = await _localStorageService.GetCartItemAsync("historyOrder");
                if (carts.Count > 0)
                {
                    CalculateTotal();
                }
                customerVouchers = await httpClient.GetFromJsonAsync<List<CustomerVoucher>>("api/CustomerVoucher/GetCustomerVoucher");
            }
            catch(Exception ex) 
            {
                await JS.InvokeVoidAsync("showAlert", "error", ex);
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
        private async Task Payment()
        {
            try
            {
                var token = await _localStorageService.GetItemAsync("n");

                int numberTable = GetTableNumberFromToken(token);
                tables = await httpClient.GetFromJsonAsync<List<Table>>("api/Table/GetTable");

                var table = tables.FirstOrDefault(a => a.TableNumber == numberTable);

                var customer_ = await httpClient.GetFromJsonAsync<List<Customer>>("api/Customer/GetCustomer");

                var customerNoAccount = customer_.FirstOrDefault(a => a.AccountId == 2);
                var AccountType = await CheckTypeAccount();

                if (!String.IsNullOrEmpty(AccountType) && AccountType.Equals("No Account") && table != null)
                {
                    order = new Order()
                    {
                        TableId = table.TableId,
                        OrderDate = DateTime.Now,
                        TotalAmount = Total,
                        Status = "Đang xử lý",
                        CustomerId = customerNoAccount.CustomerId,
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
                        await _cartService.ClearCart();
                        await Task.Delay(1000);
                        Navigation.NavigateTo("/");
                    }
                }
                else
                {
                    order = new Order()
                    {
                        TableId = table.TableNumber,
                        OrderDate = DateTime.Now,
                        TotalAmount = Total,
                        Status = "Đang xử lý",
                        CustomerId = customerNoAccount.CustomerId,
                        PaymentMethod = "",
                        CustomerVoucherId = isUseVoucher ? vcId : null
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
                }

                await JS.InvokeVoidAsync("showAlert", "success", "Gọi nhân viên thành công", "Bạn vui lòng đợi giây lát nhé");
            }
            catch(Exception ex) 
            {
                await JS.InvokeVoidAsync("showAlert", "error", "Lỗi thanh toán", ex.Message);
            }
            
        }
        private int GetTableNumberFromToken(string token)
        {
            var handler = new JwtSecurityTokenHandler();
            var jwtToken = handler.ReadToken(token) as JwtSecurityToken;
            var tableNumberClaim = jwtToken?.Claims.FirstOrDefault(c => c.Type == "tableNumber");
            return int.Parse(tableNumberClaim?.Value);
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
            else
            {
                await JS.InvokeVoidAsync("showAlert", "error", "Lỗi", "Lỗi thêm Order");
            }
        }
        private async Task SaveOrderItem(OrderItem _orderItem)
        {
            try
            {
                await httpClient.PostAsJsonAsync("api/OrderItem/AddOrderItem", _orderItem);
            }
            catch
            {
                await JS.InvokeVoidAsync("showAlert", "error", "Lỗi","Lỗi thêm OrderItem");
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
                        rewardPointe = new RewardPointe()
                        {
                            CustomerId = customer.CustomerId,
                            RewardPoint = (int)(_order.TotalAmount / 100000),
                            UpdateDate = DateTime.Now,
                            IsDeleted = false,
                            OrderId = _order.OrderId,
                        };

                        if (rewardPointes != null && rewardPointes.Count > 0)
                        {
                            var rp = rewardPointes.FirstOrDefault(rp => rp.CustomerId == customer.CustomerId);
                            if (rp != null)
                            {
                                rp.RewardPoint += (int)(_order.TotalAmount / 100000);
                                rp.OrderId = _order.OrderId;
                                rp.UpdateDate = DateTime.Now;
                                var res = await httpClient.PutAsJsonAsync($"api/RewardPointe/{rp.RewardPointId}", rp);
                                if(res.IsSuccessStatusCode)
                                {
                                    await JS.InvokeVoidAsync("showAlert", "success", "Cộng điểm thành công" ,"Đã cộng: " + (int)(_order.TotalAmount / 100000));
                                }
                            }
                            else
                            {
                                await JS.InvokeVoidAsync("showAlert", "error", "Lỗi", "rp is null");
                            }
                        }
                        else
                        {
                            var response = await httpClient.PostAsJsonAsync("api/RewardPointe/AddRewardPointe", rewardPointe);
                            if (response.IsSuccessStatusCode)
                            {
                                var createdrewardPointe = await response.Content.ReadFromJsonAsync<RewardPointe>();
                                customer.TotalRewardPoint = createdrewardPointe.RewardPoint;
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
                        await JS.InvokeVoidAsync("showAlert", "error", "Lỗi", "customer is null");
                    }
                }
                else
                {
                    await JS.InvokeVoidAsync("showAlert", "error","Lỗi" ,"customers is null");
                }
            }
            catch(Exception ex)
            {
                await JS.InvokeVoidAsync("showAlert", "error", "Lỗi", ex.Message);
            }
        }

        private async Task UseVoucher(int voucherId)
        {
            vouchers = await httpClient.GetFromJsonAsync<List<Voucher>>("api/Voucher/GetVoucher");
            var vc = vouchers.FirstOrDefault(vc => vc.VoucherId == voucherId);
            if(vc != null)
            {
                vcId = voucherId;
                Total = Total * vc.DiscountValue;
                StateHasChanged();
            }
            else
            {
                await JS.InvokeVoidAsync("showAlert", "warning", "Không tìm thấy voucher");
            }

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
