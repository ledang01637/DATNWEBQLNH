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
        private Voucher voucher = new();
        private CustomerVoucher customerVoucher = new();
        private Customer customer = new();
        private List<OrderItem> orderItems = new();
        private List<Cart> carts = new();
        private List<CustomerVoucher> customerVouchers = new();
        private bool isHasAccount = false;
        private string Code;
        private bool isCorrectVoucher = false;
        private decimal originalTotalAmount;
        private char payMenthod;

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

                string accountType = await CheckTypeAccount();

                if (!string.IsNullOrEmpty(accountType) && accountType.ToLower().Equals("no account"))
                {
                    isHasAccount = false;
                }
                else
                {
                    isHasAccount = true;
                    string accountId = await CheckAccountId();
                    if(string.IsNullOrEmpty(accountId)) { await JS.InvokeVoidAsync("showAlert", "error", "Lỗi", "Vui lòng đăng nhập lại tài khoản"); return ; }

                    customer = await GetCustomerByAccount(int.Parse(accountId));

                    if (customer != null) 
                    {
                        customerVouchers = await GetCustomerVoucherByCustomerId(customer.CustomerId);
                    }
                }
            }
            catch(Exception ex)
            {
                await JS.InvokeVoidAsync("showAlert", "warning", "Thông báo","Vui lòng thêm món: " + ex.Message);
            }
        }

        private async Task UseVoucher(string voucherCode, bool isInput)
        {
            if (isInput && string.IsNullOrEmpty(voucherCode))
            {
                await JS.InvokeVoidAsync("showAlert", "warning", "Thông báo", "Vui lòng nhập voucher");
                return;
            }

            voucher = await GetCustomerVoucherByVoucherCode(voucherCode);

            if (voucher == null || voucher.VoucherId <= 0)
            {
                Code = null;
                await JS.InvokeVoidAsync("showAlert", "error", "Thông báo", "Voucher không hợp lệ");
                return;
            }

            if (customerVouchers != null && customerVouchers.Count > 0)
            {
                customerVoucher = customerVouchers.FirstOrDefault(a => a.VoucherId == voucher.VoucherId && !a.IsUsed);

                if (customerVoucher == null && isInput)
                {
                    Code = null;
                    await JS.InvokeVoidAsync("showAlert", "warning", "Thông báo", "Voucher này không phải của bạn");
                    return;
                }

                var now = DateTime.Now;
                if (customerVoucher.ExpirationDate <= now)
                {
                    Code = null;
                    await JS.InvokeVoidAsync("showAlert", "warning", "Thông báo", "Voucher này đã hết hạn");
                    return;
                }

                if (customerVoucher.IsUsed)
                {
                    Code = null;
                    await JS.InvokeVoidAsync("showAlert", "warning", "Thông báo", "Voucher này đã được sử dụng");
                    return;
                }
            }

            Code = voucherCode;
            isCorrectVoucher = true;

            if (originalTotalAmount == 0)
            {
                originalTotalAmount = order.TotalAmount;
            }

            order.TotalAmount = originalTotalAmount - (originalTotalAmount * voucher.DiscountValue);

            await JS.InvokeVoidAsync("closeModal", "voucherModal");
        }

        private async Task<Voucher> GetCustomerVoucherByVoucherCode(string voucherCode)
        {
            var response = await httpClient.PostAsJsonAsync("api/Voucher/GetVoucherByCode", voucherCode);

            if (response.IsSuccessStatusCode)
            {
                var responseContent = await response.Content.ReadFromJsonAsync<Voucher>();

                if (responseContent != null)
                {
                    return responseContent;
                }
                return null;
            }
            else
            {
                voucher = null;
            }

            return null;
        }

        private async Task<List<CustomerVoucher>> GetCustomerVoucherByCustomerId(int customerId)
        {
            var response = await httpClient.PostAsJsonAsync("api/CustomerVoucher/GetCustomerVoucherByCustomerId", customerId);

            if (response.IsSuccessStatusCode)
            {
                var responseContent = await response.Content.ReadFromJsonAsync<List<CustomerVoucher>>();

                if (responseContent != null)
                {
                    return responseContent;
                }
                return null;
            }
            else
            {
                customerVouchers = null;
            }

            return null;
        }

        private async Task<Customer> GetCustomerByAccount(int accountId)
        {
            var response = await httpClient.PostAsJsonAsync("api/Customer/GetCustomerByAccountId", accountId);

            if (response.IsSuccessStatusCode)
            {
                var responseContent = await response.Content.ReadFromJsonAsync<Customer>();


                if (responseContent != null)
                {
                    return responseContent;
                }
                return null;
            }
            else
            {
                return null;
            }
        }

        private async Task LoadInit()
        {
            var tbid = await _localStorageService.GetItemAsync("tbid");
            if(tbid == null) { await JS.InvokeVoidAsync("showAlert", "warning", "Thông báo", "Vui thêm món ăn"); return; }
            int tableId = int.Parse(tbid);

            if(tableId <= 0) { await JS.InvokeVoidAsync("showAlert", "warning", "Thông báo", "Vui lòng quét mã QR trên bàn"); return; }

            order = await GetOrderForTable(tableId);

            if(order == null || order.OrderId <= 0) {

                await JS.InvokeVoidAsync("showAlert", "warning", "Thông báo" ,"Vui lòng đặt món ăn"); 

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
                                    UnitName = item.Products.Units.UnitName,
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

        private async Task ChoosePayMenthodAsync(char Cash)
        {
            payMenthod = Cash;
            await JS.InvokeVoidAsync("selectPaymentMethod", Cash, "cashBtnId", "transferBtnId");
        }

        private async Task<Order> GetOrderForTable(int tableId)
        {
            var order = await httpClient.GetFromJsonAsync<Order>($"api/Order/GetOrderStatus?tableId={tableId}");

            return order;
        }

        private bool IsCompleteAllProd(int tableNumber)
        {
            var notePR = NoteProdReq.noteProdReqs[tableNumber];

            if (notePR == null || notePR.ProdReqs.Count == 0)
            {
                return true;
            }

            foreach (var i in notePR.ProdReqs)
            {
                if (!i.IsComplete)
                {
                    return false;
                }
            }
            return true;
        }

        private async Task Payment()
        {
            try
            {
                if (hubConnection is not null && hubConnection.State == HubConnectionState.Connected)
                {
                    // Lấy mã token
                    string token = await _localStorageService.GetItemAsync("n");
                    if (string.IsNullOrEmpty(token))
                    {
                        await JS.InvokeVoidAsync("showAlert", "error", "Thông báo", "Vui lòng quét mã QR");
                        return;
                    }

                    int numberTable = GetTableNumberFromToken(token);

                    if (payMenthod != 'c' && payMenthod != 't')
                    {
                        await JS.InvokeVoidAsync("showAlert", "error", "Lỗi", "Vui lòng chọn phương thức thanh toán");
                        return;
                    }
                    else
                    {
                        order.PaymentMethod = (payMenthod == 'c') ? "Tiền mặt" : "Chuyển khoản";

                        if (voucher != null && voucher.VoucherId > 0)
                        {
                            if(order.Status.Equals("Đang xác nhận")){ await JS.InvokeVoidAsync("showAlert", "warning", "Thông báo", "Hóa đơn đang chờ nhân viên xác nhận bạn vui lòng đợi tý nhé"); return; }

                            order.CustomerVoucherId = customerVoucher.CustomerVoucherId;
                            order.Status = "Đang xác nhận";
                            var response = await httpClient.PutAsJsonAsync($"api/Order/{order.OrderId}", order);

                            if (response.IsSuccessStatusCode)
                            {
                                customerVoucher.IsUsed = true;
                                customerVoucher.Status = "Đã dùng";

                                var res = await httpClient.PutAsJsonAsync($"api/CustomerVoucher/{customerVoucher.CustomerVoucherId}", customerVoucher);
                                if (!res.IsSuccessStatusCode)
                                {
                                    await JS.InvokeVoidAsync("showAlert", "error", "Lỗi", "Không thể cập nhật trạng thái voucher");
                                    return;
                                }
                            }
                            else
                            {
                                await JS.InvokeVoidAsync("showAlert", "error", "Lỗi", "Không thể cập nhật đơn hàng với voucher này");
                                return;
                            }
                        }
                        else
                        {
                            var response = await httpClient.PutAsJsonAsync($"api/Order/{order.OrderId}", order);
                            if (!response.IsSuccessStatusCode)
                            {
                                await JS.InvokeVoidAsync("showAlert", "error", "Lỗi", "Cập nhật đơn hàng không thành công");
                                return;
                            }
                        }
                        if (payMenthod == 't')
                        {
                            await Transfer(order);
                            return;
                        }
                    }

                    carts.Clear();
                    await hubConnection.SendAsync("SendPay", "payReq", numberTable, order.OrderId, customer.CustomerId);
                    await JS.InvokeVoidAsync("showAlert", "success", "Thông báo", "Bạn vui lòng đợi giây lát");
                    Navigation.NavigateTo("/");
                }
                else
                {
                    await JS.InvokeVoidAsync("alert", "Không thể kết nối tới server!");
                }
            }
            catch(Exception ex)
            {
                Console.WriteLine("Lỗi: " + ex.Message);
                await JS.InvokeVoidAsync("showAlert", "error", "Lỗi thanh toán","Vui lòng liên hệ Admin");
            }
        }

        private async Task Transfer(Order order)
        {
            var vnpRequest = new VNPayRequest
            {
                OrderId = new Random().Next(10000, 99999),
                Amount = (long)order.TotalAmount,
                Description = "Thanh toán hóa đơn",
                CreatedDate = DateTime.Now,
                FullName = "no name",
            };

            var response = await httpClient.PostAsJsonAsync("api/VNPay/CreateUrlVNPay", vnpRequest);
            if (response.IsSuccessStatusCode)
            {
                var paymentUrl = await response.Content.ReadAsStringAsync();
                Navigation.NavigateTo(paymentUrl, true);
            }
            else
            {
                var errorMessage = await response.Content.ReadAsStringAsync();
                await JS.InvokeVoidAsync("showAlert", "error", "Lỗi", $"API lỗi: {errorMessage}");
            }
        }

        private int GetTableNumberFromToken(string token)
        {
            var handler = new JwtSecurityTokenHandler();
            var jwtToken = handler.ReadToken(token) as JwtSecurityToken;
            var userId = jwtToken?.Claims.FirstOrDefault(c => c.Type == "userId");
            return int.Parse(userId?.Value);
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
        private async Task<string> CheckAccountId()
        {
            var token = await _localStorageService.GetItemAsync("authToken");
            if (!String.IsNullOrEmpty(token))
            {
                var handler = new JwtSecurityTokenHandler();
                var jwtToken = handler.ReadToken(token) as JwtSecurityToken;
                return jwtToken.Claims.FirstOrDefault(c => c.Type == "AccountId")?.Value;
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
