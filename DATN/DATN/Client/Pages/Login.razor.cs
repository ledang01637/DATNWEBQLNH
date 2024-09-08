using Microsoft.JSInterop;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Json;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using DATN.Shared;
//using DATN.Client.Service;
using static System.Net.WebRequestMethods;
using System.Net.Http;



namespace DATN.Client.Pages
{
    
    public partial class Login
    {
        private HttpClient Http;
        private LoginRequest loginUser = new LoginRequest();
        private bool loginFailed = false;
        private string currentPassword = string.Empty;
        public List<DATN.Shared.User> Users = liststaticuser.Users;
        private string confirmNewPassword = string.Empty;
        private User loggedInUser = null;

        
       
        


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
            if (loggedInUser == null)
            {
                // Nếu người dùng chưa đăng nhập
                await JS.InvokeVoidAsync("showAlert", "Chưa đăng nhập");
                return;
            }
            if (passwordChangeModel.NewPassword != passwordChangeModel.ConfirmNewPassword)
            {
                // Kiểm tra nếu mật khẩu mới không khớp với xác nhận mật khẩu
                await JS.InvokeVoidAsync("showAlert", "Mật khẩu xác nhận không khớp");
                return;
            }        
            // Cập nhật mật khẩu mới
            loggedInUser.Password = passwordChangeModel.NewPassword;
            await JS.InvokeVoidAsync("showAlert", "Đổi mật khẩu thành công");
        }

        //public class EmailRequest
        //{
        //    public string ToEmail { get; set; }
        //    public string Subject { get; set; }
        //    public string Body { get; set; }
        //}

        public class PasswordChangeModel
        {
            public string CurrentPassword { get; set; }
            public string NewPassword { get; set; }
            public string ConfirmNewPassword { get; set; }
        }

        private class User
        {
            public string Username { get; set; }
            public string Email { get; set; }
            public string Password { get; set; }
            public int RoleId { get; set; }
            public DateTime CreatedAt { get; set; }
            public string AccountType { get; set; }
        }

        private class LoginRequest
        {
            public string Email { get; set; }
            public string Password { get; set; }
        }
    }
}
