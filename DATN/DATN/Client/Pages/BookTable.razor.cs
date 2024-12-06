using DATN.Shared;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.JSInterop;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Json;
using System.Threading.Tasks;

namespace DATN.Client.Pages
{
    public partial class BookTable
    {
        private readonly Reservation reservationModel = new();
        private List<Table> tables = new();
        private DateTime selectedDate = DateTime.Now;
        private DateTime selectedTime = DateTime.Now;
        private int countSeat;

        protected override async void OnInitialized()
        {
            reservationModel.Adults = 1;
            reservationModel.PaymentMethod = "Transfer";
            tables = await httpClient.GetFromJsonAsync<List<Table>>("api/Table/GetTableEmplty");
            if(tables.Count > 0)
            {
                foreach(Table table in tables)
                {
                    countSeat += table.SeatingCapacity;
                }
            }
        }

        public async Task HandleBookTableAsync()
        {
            
            try
            {

                reservationModel.TableId = 1;
                reservationModel.CreatedDate = DateTime.Now;
                reservationModel.UpdatedDate = DateTime.Now;

                reservationModel.IsDeleted = false;
                reservationModel.IsPayment = false;
                reservationModel.ReservationStatus = "Đang xử lý";


                await _localStorageService.SetAsync("reservation", reservationModel);

                if(reservationModel.PaymentMethod != "Cash")
                {
                    var vnpRequest = new VNPayRequest
                    {
                        OrderId = new Random().Next(10000, 99999),
                        Amount = (long)reservationModel.DepositPayment,
                        Description = "Thanh toán đặt bàn",
                        CreatedDate = DateTime.Now,
                        FullName = reservationModel.CustomerName,
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
                else
                {
                    var response = await httpClient.PostAsJsonAsync("api/Reservation/AddReservation", reservationModel);

                    if (!response.IsSuccessStatusCode)
                    {
                        await JS.InvokeVoidAsync("showAlert", "error", "Lỗi", "Thanh toán thất bại"); return;

                    }
                    await JS.InvokeVoidAsync("showAlert", "success", "Thông báo", "Đặt bàn thành công");
                    Navigation.NavigateTo("/", true);
                }
            }
            catch (Exception ex)
            {
                await JS.InvokeVoidAsync("showAlert", "error", "Lỗi", "Vui lòng liên hệ Admin: " + ex.Message);
                return;
            }
        }

        private Task CatulatorDepositPaymentAsync()
        {
            const decimal adultDeposit = 50000;
            const decimal childDeposit = 0;

            var adults = reservationModel.Adults;
            var children = reservationModel.Children;

            reservationModel.DepositPayment = adults * adultDeposit + children * childDeposit;
            StateHasChanged();
            return Task.CompletedTask;
        }

        private async void OnSubmitForm()
        {
            await JS.InvokeVoidAsync("closeModal", "ConformInfo");

            reservationModel.ReservationTime = new DateTime(
                    selectedDate.Year,
                    selectedDate.Month,
                    selectedDate.Day,
                    selectedTime.Hour,
                    selectedTime.Minute,
                    0
                );

            if (reservationModel.ReservationTime < DateTime.Now)
            {
                await JS.InvokeVoidAsync("showAlert", "warning", "Thông báo", "Thời gian đặt bàn phải lớn hơn thời gian hiện tại.");
                return;
            }

            if (reservationModel.ReservationTime < DateTime.Now.AddHours(2).AddMinutes(30))
            {
                await JS.InvokeVoidAsync("showAlert", "warning", "Thông báo", "Thời gian đặt bàn phải ít nhất sau 2 giờ 30 phút kể từ hiện tại.");
                return;
            }

            var totalCustomer = reservationModel.Adults + reservationModel.Children;

            if (totalCustomer > countSeat)
            {
                await JS.InvokeVoidAsync("showAlert", "warning", "Thông báo", "Hiện tại nhà hàng chỉ còn: " + countSeat + " chỗ");
                return;
            }

            if(countSeat <= 0) 
            {
                await JS.InvokeVoidAsync("showAlert", "warning", "Thông báo", "Hiện đã hết bàn trống vui lòng quay lại sau");
                return;
            }
            

            await JS.InvokeVoidAsync("showModal", "ConformInfo");
            await CatulatorDepositPaymentAsync();
        }

        private async Task ChoosePayMenthodAsync(char Cash)
        {
            reservationModel.PaymentMethod = Cash == 'c' ? "Cash" : "Transfer";
            await JS.InvokeVoidAsync("selectPaymentMethod", Cash, "cashBtnId", "transferBtnId");
        }
    }
}
