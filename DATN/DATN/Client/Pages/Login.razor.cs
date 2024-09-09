using Microsoft.JSInterop;
using System;
using System.Collections.Generic;
using System.Net.Http.Json;
using System.Threading.Tasks;
using DATN.Shared;
using System.Net.Http;



namespace DATN.Client.Pages
{
    
    public partial class Login
    {
        private LoginRequest loginUser = new LoginRequest();
        private bool loginFailed = false;
        private string currentPassword = string.Empty;
        private string confirmNewPassword = string.Empty;


        private async Task HandleValidSubmit()
        {
            var response = await httpClient.PostAsJsonAsync("api/AuthJWT/AuthUser", loginUser);

            if (response.IsSuccessStatusCode)
            {
                // Đăng nhập thành công
                loginFailed = false;
                await JS.InvokeVoidAsync("showAlert", "True");

            }
            else
            {
                // Đăng nhập thất bại
                loginFailed = true;
                await JS.InvokeVoidAsync("showAlert", "False");
            }
        }

        


        private async Task HandlePasswordChange()
        {
            //if (loggedInUser == null)
            //{
            //    // Nếu người dùng chưa đăng nhập
            //    await JS.InvokeVoidAsync("showAlert", "Chưa đăng nhập");
            //    return;
            //}
            //if (passwordChangeModel.NewPassword != passwordChangeModel.ConfirmNewPassword)
            //{
            //    // Kiểm tra nếu mật khẩu mới không khớp với xác nhận mật khẩu
            //    await JS.InvokeVoidAsync("showAlert", "Mật khẩu xác nhận không khớp");
            //    return;
            //}        
            //// Cập nhật mật khẩu mới
            //loggedInUser.Password = passwordChangeModel.NewPassword;
            //await JS.InvokeVoidAsync("showAlert", "Đổi mật khẩu thành công");
        }
    }
}
