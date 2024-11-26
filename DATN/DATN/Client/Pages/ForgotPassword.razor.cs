using DATN.Shared;
using Microsoft.JSInterop;
using System;
using System.Net.Http.Json;
using System.Threading.Tasks;
namespace DATN.Client.Pages
{
    public partial class ForgotPassword
    {
        private EmailRequest emailRequest = new EmailRequest();
        private string errorMessage;

        private async Task SendOtp()
        {
            try
            {
                var response = await Http.PostAsJsonAsync("api/ForgotPassword/SendOtp", emailRequest);

                if (response.IsSuccessStatusCode)
                {
                    await JSRuntime.InvokeVoidAsync("Swal.fire", new
                    {
                        title = "Thành công",
                        text = "Một mã OTP đã được gửi vào email của bạn.",
                        icon = "success",
                        confirmButtonText = "OK"
                    });
                    Navigation.NavigateTo("/resetpassword");
                    errorMessage = null;

                }
                else
                {
                    var errorMessage = await response.Content.ReadAsStringAsync();
                    Console.WriteLine($"Error: {errorMessage}");

                    await JSRuntime.InvokeVoidAsync("Swal.fire", new
                    {
                        title = "Lỗi",
                        text = "Email bị bỏ trống hoặc không tồn tại.",
                        icon = "error",
                        confirmButtonText = "OK"
                    });
                }
            }
            catch (Exception ex)
            {
                errorMessage = $"Lỗi khi gửi OTP: {ex.Message}";
            }
        }
    }
}