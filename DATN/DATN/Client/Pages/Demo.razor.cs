using DATN.Shared;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.JSInterop;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;

namespace DATN.Client.Pages
{
    public partial class Demo
    {
        private async Task CreatePayment()
        {
            var vnpRequest = new VNPayRequest
            {
                OrderId = new Random().Next(10000, 99999),
                Amount = 50000m / 100m,
                Description = "Thanh toán đặt bàn",
                CreatedDate = DateTime.Now,
                FullName = "abc"
            };

            var response = await httpClient.PostAsJsonAsync("api/VNPay/CreateUrlVNPay", vnpRequest);
            if(response.IsSuccessStatusCode)
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
    }
}
