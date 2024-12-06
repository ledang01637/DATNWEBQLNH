using DATN.Shared;
using Microsoft.JSInterop;
using System;
using System.ComponentModel.DataAnnotations;
using System.Net.Http.Json;
using System.Threading.Tasks;
namespace DATN.Client.Pages
{
    public partial class ResetPassword
    {
        private ResetPasswordRequest resetPasswordRequest = new ResetPasswordRequest();
        private string successMessage;
        private string errorMessage;

        private async Task ResetPasswordd()
        {
            try
            {
                var response = await Http.PostAsJsonAsync("api/ForgotPassword/ResetPasswordd", resetPasswordRequest);

                if (response.IsSuccessStatusCode)
                {
                    await JSRuntime.InvokeVoidAsync("Swal.fire", new
                    {
                        title = "Thành công",
                        text = "Mật khẩu đã được thay đổi thành công!",
                        icon = "success",
                        confirmButtonText = "OK"
                    });
                    Navigation.NavigateTo("/Login");
                }
                else
                {
                    var content = await response.Content.ReadAsStringAsync();
                    errorMessage = content;
                    successMessage = null;
                    await JSRuntime.InvokeVoidAsync("Swal.fire", new
                    {
                        title = "Lỗi",
                        text = errorMessage,
                        icon = "error",
                        confirmButtonText = "OK"
                    });
                }
            }
            catch (Exception ex)
            {
                errorMessage = $"Lỗi khi thay đổi mật khẩu: {ex.Message}";
                successMessage = null;
                await JSRuntime.InvokeVoidAsync("Swal.fire", new
                {
                    title = "Lỗi",
                    text = errorMessage,
                    icon = "error",
                    confirmButtonText = "OK"
                });
            }
        }
    }
}
