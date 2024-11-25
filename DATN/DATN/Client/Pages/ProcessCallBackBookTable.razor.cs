using DATN.Shared;
using System.Threading.Tasks;
using System;
using System.Net.Http.Json;
using Microsoft.JSInterop;

namespace DATN.Client.Pages
{
    public partial class ProcessCallBackBookTable
    {
        protected override async Task OnInitializedAsync()
        {
            try
            {
                var reservation = await _localStorageService.GetAsync<Reservation>("reservation");
                var currentUrl = Navigation.ToAbsoluteUri(Navigation.Uri);

                var responseVnPay = await httpClient.GetFromJsonAsync<VNPayResponse>($"api/VNPay/PaymentCallBack{currentUrl.Query}");

                if (responseVnPay.VnPayResponseCode != "00")
                {
                    await JS.InvokeVoidAsync("showAlert", "error", "Lỗi", "Thanh toán thất bại");
                    Navigation.NavigateTo("/book-table");
                    return;
                }
                else
                {
                    var response = await httpClient.PostAsJsonAsync("api/Reservation/AddReservation", reservation);

                    if(response.IsSuccessStatusCode)
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

                    await JS.InvokeVoidAsync("showAlert", "success", "Thông báo", "Thanh toán thành công");
                }
            }
            catch (Exception ex)
            {
                await JS.InvokeVoidAsync("showAlert", "error", "Lỗi", "Vui lòng liên hệ Admin: " + ex.Message);
            }
            finally
            {
                StateHasChanged();
            }

        }
    }
}
