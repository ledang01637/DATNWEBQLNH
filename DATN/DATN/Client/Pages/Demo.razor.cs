using DATN.Client.Pages.AdminManager;
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

        private Reservation reservation = new();

        private async Task CreatePayment()
        {
            reservation = await _localStorageService.GetAsync<Reservation>("reservaion");
            var vnpRequest = new VNPayRequest
            {
                OrderId = new Random().Next(10000, 99999),
                Amount = reservation.Adults * 50000,
                Description = "Thanh toán đặt bàn",
                CreatedDate = DateTime.Now,
                FullName = reservation.CustomerName,
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

    }
}
