using DATN.Shared;
using Microsoft.JSInterop;
using System;
using System.Net.Http.Json;
using System.Threading.Tasks;

namespace DATN.Client.Pages
{
    public partial class PasswordChange
    {
        private PasswordChangeRequest changepassRequest = new PasswordChangeRequest();
        private bool isOtpSent = false;
        private string errorMessage;

        private async Task SendOtp()
        {
            try
            {
                if (!isOtpSent)
                {
                    var otpResponse = await Http.PostAsJsonAsync("api/ForgotPassword/SendOtp", changepassRequest);

                    if (otpResponse.IsSuccessStatusCode)
                    {
                        isOtpSent = true;
                        await JSRuntime.InvokeVoidAsync("Swal.fire", new
                        {
                            title = "Thành công",
                            text = "Mã OTP đã được gửi vào email của bạn.",
                            icon = "success",
                            confirmButtonText = "OK"
                        });
                    }
                    else
                    {
                        var content = await otpResponse.Content.ReadAsStringAsync();

                        await JSRuntime.InvokeVoidAsync("Swal.fire", new
                        {
                            title = "Lỗi",
                            text = content,
                            icon = "error",
                            confirmButtonText = "OK"
                        });
                    }
                }
                else
                {
                    var resetResponse = await Http.PostAsJsonAsync("api/ForgotPassword/ResetPasswordd", changepassRequest);

                    if (resetResponse.IsSuccessStatusCode)
                    {
                        await JSRuntime.InvokeVoidAsync("Swal.fire", new
                        {
                            title = "Thành công",
                            text = "Mật khẩu đã được thay đổi thành công.",
                            icon = "success",
                            confirmButtonText = "OK"
                        });

                        Navigation.NavigateTo("/Login");
                    }
                    else
                    {

                        var content = await resetResponse.Content.ReadAsStringAsync();

                        await JSRuntime.InvokeVoidAsync("Swal.fire", new
                        {
                            title = "Lỗi",
                            text = content,
                            icon = "error",
                            confirmButtonText = "OK"
                        });
                    }
                }
            }
            catch (Exception ex)
            {
                errorMessage = $"Có lỗi xảy ra: {ex.Message}";
            }
        }
    }
}
