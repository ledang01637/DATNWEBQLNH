using DATN.Shared;
using System.Threading.Tasks;
using System;
using System.Net.Http.Json;
using Microsoft.JSInterop;
using Microsoft.AspNetCore.SignalR.Client;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using Newtonsoft.Json.Serialization;

namespace DATN.Client.Pages
{
    public partial class ProcessCallBackBookTable
    {
        private Order order = new();
        private Customer customer = new();
        private List<OrderItem> orderItems = new();
        private List<Cart> carts = new();

        private HubConnection hubConnection;
        protected override async Task OnInitializedAsync()
        {
            try
            {
                hubConnection = new HubConnectionBuilder()
               .WithUrl(Navigation.ToAbsoluteUri("/ProcessHub"))
               .Build();

                await hubConnection.StartAsync();

                var currentUrl = Navigation.ToAbsoluteUri(Navigation.Uri);

                var responseVnPay = await httpClient.GetFromJsonAsync<VNPayResponse>($"api/VNPay/PaymentCallBack{currentUrl.Query}");

                if (responseVnPay.VnPayResponseCode != "00")
                {
                    await JS.InvokeVoidAsync("showAlert", "error", "Lỗi", "Thanh toán thất bại");

                    if (responseVnPay.OrderDescription == "Thanh toán hóa đơn")
                    {
                        Navigation.NavigateTo("/food-ordered");
                        return;
                    }
                    else
                    {
                        Navigation.NavigateTo("/book-table");
                        return;
                    }

                }
                else
                {
                    if(responseVnPay.OrderDescription == "Thanh toán hóa đơn")
                    {
                        await ProcessPayment();
                    }
                    else
                    {
                        await ProcessBookTable();
                    }
                }
            }
            catch
            {
                await JS.InvokeVoidAsync("showAlert", "error", "Lỗi", "Thanh toán thất bại");
            }
            finally
            {
                StateHasChanged();
            }

        }

        private async Task ProcessBookTable()
        {
            var reservation = await _localStorageService.GetAsync<Reservation>("reservation");
            var response = await httpClient.PostAsJsonAsync("api/Reservation/AddReservation", reservation);

            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadFromJsonAsync<Reservation>();

                var transac = new Transaction()
                {
                    ReservationId = content.ReservationId,
                    Amount = reservation.DepositPayment,
                    PaymentStatus = reservation.PaymentMethod,
                    PaymentDate = DateTime.Now,
                };

                var resTransaction = await httpClient.PostAsJsonAsync("api/Transaction/AddTransaction", transac);

                if (!resTransaction.IsSuccessStatusCode) { await JS.InvokeVoidAsync("showAlert", "error", "Lỗi", "Thanh toán thất bại"); return; }

            }

            await JS.InvokeVoidAsync("showAlert", "success", "Thông báo", "Đặt bàn thành công");
            Navigation.NavigateTo("/");
            return;
        }
        private async Task ProcessPayment()
        {
            try
            {
                if (hubConnection is not null && hubConnection.State == HubConnectionState.Connected)
                {
                    string accountId = await CheckAccountId();
                    if (string.IsNullOrEmpty(accountId)) { await JS.InvokeVoidAsync("showAlert", "error", "Lỗi", "Vui lòng đăng nhập lại tài khoản"); return; }

                    customer = await GetCustomerByAccount(int.Parse(accountId));


                    var tbid = await _localStorageService.GetItemAsync("tbid");

                    if (tbid == null) { await JS.InvokeVoidAsync("showAlert", "warning", "Thông báo", "Vui quét mã QR lại"); return; }

                    int tableId = int.Parse(tbid);

                    if (tableId <= 0) { await JS.InvokeVoidAsync("showAlert", "warning", "Thông báo", "Vui lòng quét mã QR trên bàn"); return; }

                    order = await GetOrderForTable(tableId, "unpaid");

                    if (order.OrderId <= 0)
                    {
                        Console.WriteLine(tableId);
                        await JS.InvokeVoidAsync("showAlert", "error", "Lỗi", "Không tìm thấy hóa đơn");
                        return;
                    }

                    string token = await _localStorageService.GetItemAsync("n");

                    if (string.IsNullOrEmpty(token))
                    {
                        await JS.InvokeVoidAsync("showAlert", "error", "Thông báo", "Vui lòng quét mã QR");
                        return;
                    }

                    int numberTable = GetTableNumberFromToken(token);

                    order.PaymentMethod = "Chuyển khoản";
                    order.Status = "unconfirmed";

                    var response = await httpClient.PutAsJsonAsync($"api/Order/{order.OrderId}", order);

                    if (!response.IsSuccessStatusCode)
                    {
                        await JS.InvokeVoidAsync("showAlert", "error", "Lỗi", "Cập nhật đơn hàng không thành công");
                        return;
                    }
                    carts.Clear();

                    await hubConnection.SendAsync("SendPay", "payReq", numberTable, order.OrderId, customer.CustomerId);

                    await JS.InvokeVoidAsync("showAlert", "success", "Thông báo", "Bạn vui lòng đợi giây lát");

                    Navigation.NavigateTo("/", true);
                }
                else
                {
                    await JS.InvokeVoidAsync("showAlert", "error","Lỗi","Không thể kết nối tới server!");
                }
            }
            catch(Exception ex)
            {
                Console.WriteLine(ex.Message);
                await JS.InvokeVoidAsync("showAlert", "error", "Lỗi thanh toán", "Vui lòng liên hệ Admin");
            }
        }
        private static int GetTableNumberFromToken(string token)
        {
            var handler = new JwtSecurityTokenHandler();
            var jwtToken = handler.ReadToken(token) as JwtSecurityToken;
            var userId = jwtToken?.Claims.FirstOrDefault(c => c.Type == "userId");
            return int.Parse(userId?.Value);
        }

        private async Task<Order> GetOrderForTable(int tableId, string status)
        {
            var order = await httpClient.GetFromJsonAsync<Order>($"api/Order/GetOrderStatus?tableId={tableId}&status={status}");
            return order;
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
        public async ValueTask DisposeAsync()
        {
            if (hubConnection is not null)
            {
                await hubConnection.DisposeAsync();
            }
        }
    }
}
