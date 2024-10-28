using DATN.Client.Service;
using DATN.Shared;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.JSInterop;
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
        private decimal Total;


        protected override async Task OnInitializedAsync()
        {
            hubConnection = new HubConnectionBuilder()
                .WithUrl(Navigation.ToAbsoluteUri("/ProcessHub"))
                .Build();

            await hubConnection.StartAsync();
            Carts = await _cartService.GetCartAsync();
            CalculateTotal();
        }

        private async Task RemoveFromCart(Cart product)
        {
            if (product != null)
            {
                if (product.Quantity > 1)
                {
                    product.Quantity -= 1;
                    await _cartService.SaveCartAsync(Carts);
                }
                else
                {
                    await _cartService.RemoveItemFromCartAsync(product);
                }
                Carts = await _cartService.GetCartAsync();
                CalculateTotal();
            }
        }

        private void CalculateTotal()
        {
            Total = 0;
            foreach (var item in Carts)
            {
                Total += item.Price * item.Quantity;
            }

        }
        private async Task Order()
        {
            if (Carts.Count > 0)
            {
                var expiryTime = DateTime.Now.AddMinutes(120).ToString("o");
                await _localStorageService.SetListAsync("historyOrder", Carts);
                await _localStorageService.SetItemAsync("cartExpiryTime", expiryTime);
                await SenMessageAsync();
            }
            else
            {
                await JS.InvokeVoidAsync("showAlert", "warning", "Vui lòng thêm món ăn", "");
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

            int number = GetTableNumberFromToken(token);
            List<Table> tables = new();
            tables = await httpClient.GetFromJsonAsync<List<Table>>("api/Table/GetTable");

            if(tables != null)
            {
                var table = tables.FirstOrDefault(t => t.TableNumber == number);
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
                                ProductId = Carts[i].ProductId,
                                UnitId = Carts[i].UnitId,
                                ProductName = Carts[i].ProductName,
                                Price = Carts[i].Price,
                                Quantity = Carts[i].Quantity
                            };
                            carts.Add(cartDto);
                        }
                        await hubConnection.SendAsync("SendTable", table.TableNumber.ToString() , carts, ListCartDTO.Note);
                        await JS.InvokeVoidAsync("showAlert", "success", "Đặt món thành công", "Bạn vui lòng đợi đầu bếp làm nha :3");
                    }
                }
                else
                {
                    await JS.InvokeVoidAsync("showAlert", "error", "Không thể kết nối tới server!");
                    return;
                }
            }
            else
            {
                await JS.InvokeVoidAsync("showAlert", "warning", "table is null");
                return;
            }
        }
        public async ValueTask DisposeAsync()
        {
            if (hubConnection is not null)
            {
                await hubConnection.DisposeAsync();
            }
        }
        private static int GetTableNumberFromToken(string token)
        {
            var handler = new JwtSecurityTokenHandler();
            var jwtToken = handler.ReadToken(token) as JwtSecurityToken;
            var userId = jwtToken?.Claims.FirstOrDefault(c => c.Type == "userId");
            return int.Parse(userId?.Value);
        }
    }
}
