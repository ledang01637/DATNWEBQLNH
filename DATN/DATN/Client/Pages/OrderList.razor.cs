using DATN.Client.Service;
using DATN.Shared;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.EntityFrameworkCore;
using Microsoft.JSInterop;
using Newtonsoft.Json.Serialization;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Net.Http.Json;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;


namespace DATN.Client.Pages
{
    public partial class OrderList
    {
        private HubConnection hubConnection;
        private List<Cart> Carts = new();
        private List<Customer> customers = new();
        private Customer customer = new();
        private List<Table> tables = new();
        private Order order = new();

        private bool isSaveOrder = false;
        private bool isUseVoucher = false;
        private int vcId;
        private int TotalQuantity;
        private decimal Total;



        protected override async Task OnInitializedAsync()
        {
            hubConnection = new HubConnectionBuilder()
                .WithUrl(Navigation.ToAbsoluteUri("/ProcessHub"))
                .Build();

            await hubConnection.StartAsync();
            Carts = await _cartService.GetCartAsync();
            await UpdateCartTotals();
        }

        #region Cart

        private async Task RemoveFromCart(Cart product)
        {
            var existingCart = Carts.FirstOrDefault(c => c.ProductId == product.ProductId);

            if (existingCart != null)
            {
                Carts.Remove(existingCart);
                _ = _cartService.SaveCartAsync(Carts);
                await UpdateCartTotals(-product.Price * product.Quantity, -product.Quantity);
            }
        }

        private async Task IncreaseQuantity(int productId)
        {
            await ModifyQuantity(productId, 1);
        }

        private async Task DecreaseQuantity(int productId)
        {
            await ModifyQuantity(productId, -1);
        }
        private async Task ModifyQuantity(int productId, int change)
        {
            var cartItem = Carts.FirstOrDefault(c => c.ProductId == productId);
            if (cartItem == null)
            {
                await JS.InvokeVoidAsync("showAlert", "warning", "Vui lòng thêm món ăn");
                return;
            }

            cartItem.Quantity += change;

            if (cartItem.Quantity <= 0)
            {
                Carts.Remove(cartItem);
            }

            await UpdateCartTotals(cartItem.Price, change);
            await _cartService.SaveCartAsync(Carts);
        }
        private Task UpdateCartTotals()
        {
            TotalQuantity = Carts.Sum(c => c.Quantity);
            Total = Carts.Sum(c => c.Price * c.Quantity);
            StateHasChanged();
            return Task.CompletedTask;
        }

        private Task UpdateCartTotals(decimal priceChange, int quantityChange)
        {
            TotalQuantity += quantityChange;

            if (priceChange < 0)
            {
                Total += priceChange;
                StateHasChanged();
                return Task.CompletedTask;
            }

            Total += priceChange * quantityChange;

            StateHasChanged();
            return Task.CompletedTask;
        }

        #endregion

        #region Order
        private async Task Order()
        {
            await JS.InvokeVoidAsync("closeModal", "ConfirmOrderModal");
            if (Carts != null && Carts.Count > 0)
            {
                try
                {
                    string token = await _localStorageService.GetItemAsync("n");

                    if (string.IsNullOrEmpty(token)) { await JS.InvokeVoidAsync("showAlert", "error", "Vui lòng quét mã QR"); return; }

                    int numberTable = GetTableNumberFromToken(token);

                    await LoadTables();

                    Table table = tables.FirstOrDefault(a => a.TableNumber == numberTable) ?? throw new Exception("Không tìm thấy bàn!");

                    var accountType = await CheckTypeAccount();
                    string noaccount = accountType != null
                        ? new string(accountType.Where(c => !char.IsWhiteSpace(c)).ToArray()).ToLower()
                        : null;

                    if (noaccount == "noaccount")
                    {
                        await HandleNoAccountPayment(table);
                    }
                    else
                    {
                        await HandleAccountPayment(table);
                    }
                    
                    await SenMessageAsync();

                }
                catch (Exception ex)
                {
                    await JS.InvokeVoidAsync("showAlert", "error", "Lỗi", "Vui lòng liên hệ Admin: " + ex.Message);
                }
            }
            else
            {
                await JS.InvokeVoidAsync("showAlert", "warning", "Thông báo", "Vui lòng thêm món ăn");
            }
        }
        private async Task SenMessageAsync()
        {
            
            string token = await _localStorageService.GetItemAsync("n");
            if (string.IsNullOrEmpty(token))
            {
                await JS.InvokeVoidAsync("showAlert", "error", "Vui lòng quét mã QR");
                return;
            }

            if(customer == null)
            {
                await JS.InvokeVoidAsync("showAlert", "error", "Không tìm thấy khách hàng");
                return;
            }


            int number = GetTableNumberFromToken(token);
            List<Table> tables = new();
            tables = await httpClient.GetFromJsonAsync<List<Table>>("api/Table/GetTable");

            if(tables != null)
            {
                var table = tables.FirstOrDefault(t => t.TableNumber == number);
                await _localStorageService.SetItemAsync("tbid",table.TableId.ToString());
                if (hubConnection is not null && hubConnection.State == HubConnectionState.Connected)
                {
                    if (table is not null && Carts.Count > 0)
                    {
                        List<CartDTO> carts = new();

                        for (int i = 0; i < Carts.Count; i++)
                        {
                            var cartDto = new CartDTO
                            {
                                TableNumber = table.TableNumber,
                                TableId = table.TableId,
                                ProductId = Carts[i].ProductId,
                                UnitName = Carts[i].UnitName,
                                ProductName = Carts[i].ProductName,
                                Price = Carts[i].Price,
                                Quantity = Carts[i].Quantity
                            };
                            carts.Add(cartDto);
                        }
                        if(table.Status != "inusebooktable")
                        {
                            table.Status = "occupied";
                        }

                        var response = await httpClient.PutAsJsonAsync($"api/Table/{table.TableId}", table);

                        if (!response.IsSuccessStatusCode) { await JS.InvokeVoidAsync("showAlert", "warning", "Thông báo", "Không thể cập nhật bàn"); return; }

                        await hubConnection.SendAsync("SendTable", table.TableNumber.ToString() , carts, ListCartDTO.Note);

                        await JS.InvokeVoidAsync("showAlert", "success", "Đặt món thành công", "Bạn vui lòng đợi đầu bếp làm");
                        Carts.Clear();
                        await _cartService.SaveCartAsync(Carts);
                        Navigation.NavigateTo("/");
                    }
                }
                else
                {
                    await JS.InvokeVoidAsync("showAlert", "error","Lỗi" ,"Không thể kết nối tới server!");
                    return;
                }
            }
            else
            {
                await JS.InvokeVoidAsync("showAlert", "warning","Thông báo" ,"Vui lòng quét mã QR");
                return;
            }
        }

        private async Task HandleNoAccountPayment(Table table)
        {
            customer = await GetCustomer("no account");
            if(customer == null) { await JS.InvokeVoidAsync("showAlert","error","Lỗi","Không tìm thấy khách hàng"); return; }

            order = await GetProcessingOrderForTable(table.TableId);

            order ??= new Order
            {
                TableId = table.TableId,
                EmployeeId = 1,
                CreateDate = DateTime.Now,
                TotalAmount = Total, 
                Status = "Tạo hóa đơn",
                CustomerId = customer.CustomerId,
                PaymentMethod = "",
                Note = string.IsNullOrEmpty(ListCartDTO.Note) ? "" : ListCartDTO.Note,
                CustomerVoucherId = null,
                IsDeleted = false
            };


            await ProcessOrder(order);
        }
        private async Task HandleAccountPayment(Table table)
        {
            var email = await GetEmailAccount();

            if(string.IsNullOrEmpty(email)) { await JS.InvokeVoidAsync("showAlert", "warning", "Thông báo", "Vui lòng đăng nhập lại"); return; }

            customer = await GetCustomer(email);

            if (customer == null)
            {
                await JS.InvokeVoidAsync("showAlert", "error", "Lỗi", "Không tìm thấy khách hàng");
                return;
            }
          
            order = await GetProcessingOrderForTable(table.TableId);

            order ??= new Order
            {
                TableId = table.TableId,
                EmployeeId = 1,
                CreateDate = DateTime.Now,
                TotalAmount = Total,
                Status = "Tạo hóa đơn",
                CustomerId = customer.CustomerId,
                PaymentMethod = "",
                Note = string.IsNullOrEmpty(ListCartDTO.Note) ? "" : ListCartDTO.Note,
                CustomerVoucherId = isUseVoucher ? vcId : null,
                IsDeleted = false
            };

            await ProcessOrder(order);
        }

        private async Task<Order> GetProcessingOrderForTable(int tableId)
        {
            var response = await httpClient.PostAsJsonAsync("api/Order/GetOrderStatus", tableId);

            if (response.IsSuccessStatusCode)
            {
                var responseContent = await response.Content.ReadFromJsonAsync<Order>();

                if(responseContent != null && responseContent.Status.Equals("Đang xử lý"))
                {
                    return responseContent;
                }
                return null;
            }
            else
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                await JS.InvokeVoidAsync("showAlert","error","Lỗi", $"Lỗi khi gọi API: {response.StatusCode} - Nội dung: {errorContent}");
            }

            return null;
        }

        private async Task ProcessOrder(Order order)
        {
            await SaveOrder(order);

            if (isSaveOrder && Carts != null && Carts.Count > 0)
            {
                foreach (var item in Carts)
                {
                    var existingOrderItem = await GetOrderItemByProductAndOrderId(order.OrderId, item.ProductId);

                    if (existingOrderItem != null && existingOrderItem.OrderItemId > 0)
                    {
                        existingOrderItem.Quantity += item.Quantity;
                        existingOrderItem.TotalPrice = existingOrderItem.Quantity * existingOrderItem.Price;
                        await UpdateOrderItem(existingOrderItem,existingOrderItem.OrderItemId);
                    }
                    else
                    {
                        await SaveOrderItem(new OrderItem
                        {
                            ProductId = item.ProductId,
                            OrderId = order.OrderId,
                            Quantity = item.Quantity,
                            Price = item.Price,
                            TotalPrice = item.Quantity * item.Price,
                            IsDeleted = false
                        });
                    }
                }
                await _cartService.ClearCart();
            }
        }
        private async Task SaveOrder(Order _order)
        {
            if (order != null && order.Status == "Đang xử lý")
            {
                order.TotalAmount += Total;
                var updateOrder  = await httpClient.PutAsJsonAsync($"api/Order/{order.OrderId}", order);
                isSaveOrder = true;
                if (!updateOrder.IsSuccessStatusCode)
                {
                    await JS.InvokeVoidAsync("showAlert", "error", "Lỗi", "Vui lòng liên hệ Admin");
                }
                return;
            }
            _order.Status = "Đang xử lý";
            var response = await httpClient.PostAsJsonAsync("api/Order/AddOrder", _order);

            if (response.IsSuccessStatusCode)
            {
                _order.OrderId = (await response.Content.ReadFromJsonAsync<Order>()).OrderId;
                isSaveOrder = true;
            }
            else
            {
                await JS.InvokeVoidAsync("showAlert", "error", "Lỗi","Vui lòng liên hệ Admin");
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
        private async Task<OrderItem> GetOrderItemByProductAndOrderId(int orderId, int productId)
        {
            var response = await httpClient.GetFromJsonAsync<OrderItem>($"api/OrderItem/GetByOrderIdAndProductId?orderId={orderId}&productId={productId}");
            return response;
        }

        private async Task UpdateOrderItem(OrderItem orderItem,int idOrderItem)
        {
            try
            {
                await httpClient.PutAsJsonAsync($"api/OrderItem/{idOrderItem}", orderItem);
            }
            catch (Exception ex)
            {
                throw new Exception("Lỗi khi cập nhật sản phẩm trong đơn hàng: " + ex.Message);
            }
        }
        private async Task LoadTables()
        {
            tables = await httpClient.GetFromJsonAsync<List<Table>>("api/Table/GetTable");
        }

        private async Task<Customer> GetCustomer(string email)
        {
            customers = await httpClient.GetFromJsonAsync<List<Customer>>("api/Customer/GetCustomerInclude");
            if(customers == null) {return null; }
            return customers.FirstOrDefault(a => a.Accounts.Email.ToLower().Equals(email));
        }

        private async Task<string> CheckTypeAccount()
        {
            var token = await _localStorageService.GetItemAsync("authToken");
            if (!string.IsNullOrEmpty(token))
            {
                var handler = new JwtSecurityTokenHandler();

                if (handler.ReadToken(token) is JwtSecurityToken jwtToken)
                {
                    var accountTypeClaim = jwtToken.Claims.FirstOrDefault(c => c.Type.Equals("AccountType"));
                    return accountTypeClaim?.Value;
                }
                else
                {
                    await JS.InvokeVoidAsync("showAlert","error","Lỗi","Vui lòng liên hệ Admin");
                }
            }
            return null;
        }

        private async Task<string> GetEmailAccount()
        {
            var token = await _localStorageService.GetItemAsync("authToken");
            if (!string.IsNullOrEmpty(token))
            {
                var handler = new JwtSecurityTokenHandler();

                if (handler.ReadToken(token) is JwtSecurityToken jwtToken)
                {
                    var accountEmailClaim = jwtToken.Claims.FirstOrDefault(c => c.Type.Equals("Username"));
                    return accountEmailClaim?.Value;
                }
                else
                {
                    await JS.InvokeVoidAsync("showAlert", "error", "Lỗi", "Vui lòng liên hệ Admin");
                }
            }
            return null;
        }

        private static int GetTableNumberFromToken(string token)
        {
            var handler = new JwtSecurityTokenHandler();
            var jwtToken = handler.ReadToken(token) as JwtSecurityToken;
            var userId = jwtToken?.Claims.FirstOrDefault(c => c.Type == "userId");
            return int.Parse(userId?.Value);
        }
        #endregion
        public async ValueTask DisposeAsync()
        {
            if (hubConnection is not null)
            {
                await hubConnection.DisposeAsync();
            }
        }

    }
}
