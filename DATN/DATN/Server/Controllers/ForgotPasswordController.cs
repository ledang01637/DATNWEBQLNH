using DATN.Server.Data;
using DATN.Server.Service;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Linq;
using MimeKit;
using MailKit.Net.Smtp;
using System.Security.Cryptography;
using System.Text;
using DATN.Shared;
using System.Net.Mail;

namespace DATN.Server.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ForgotPasswordController : ControllerBase
    {
        private readonly AppDBContext _context;
        private readonly CustomerService _CustomerService;
        public ForgotPasswordController(AppDBContext context, CustomerService customerService)

        {
            _context = context;
            _CustomerService = customerService;
        }
        [HttpPost("SendOtp")]
        public IActionResult SendOtp([FromBody] EmailRequestForgetPass request)
        {
            try
            {
                if (string.IsNullOrEmpty(request.Email))
                {
                    return BadRequest("Email không được bỏ trống.");
                }

                var customer = _CustomerService.GetCustomerByEmail(request.Email);
                if (customer == null)
                {
                    return BadRequest("Email không tồn tại trong hệ thống.");
                }

                var otp = GenerateOtp();
                HttpContext.Session.SetString("otp", otp);
                HttpContext.Session.SetString("storedEmail", request.Email);

                SendOtpEmail(request.Email, otp);

                return Ok("OTP đã gửi thành công vào email của bạn.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }
        private string GenerateOtp()
        {
            Random random = new Random();
            return random.Next(100000, 999999).ToString();
        }
        private string ConvertToSHA1(string password)
        {
            using (SHA1 sha1 = SHA1.Create())
            {
                byte[] passwordBytes = Encoding.UTF8.GetBytes(password);
                byte[] hashedBytes = sha1.ComputeHash(passwordBytes);
                password = Convert.ToBase64String(hashedBytes);
            }
            return password;
        }

        [HttpPost("ResetPasswordd")]
        public IActionResult ResetPasswordd([FromBody] ResetPasswordRequest request)
        {


            var sessionOtp = HttpContext.Session.GetString("otp");
            var storedEmail = HttpContext.Session.GetString("storedEmail");

            if (string.IsNullOrEmpty(sessionOtp) || sessionOtp != request.Otp)
            {
                return BadRequest("Mã OTP không hợp lệ.");
            }

            if (string.IsNullOrEmpty(storedEmail))
            {
                return BadRequest("Email không tồn tại hoặc đã hết hạn.");
            }
            var customer = _CustomerService.GetCustomerByEmail(storedEmail);
            if (customer == null)
            {
                return BadRequest("Không tìm thấy tài khoản với email này.");
            }
            if (string.IsNullOrEmpty(request.NewPassword))
            {
                return BadRequest("Mật khẩu mới không được để trống.");
            }
            if (request.NewPassword.Length < 6)
            {
                return BadRequest("Mật khẩu mới phải có ít nhất 6 ký tự.");
            }

            if (request.NewPassword != request.ConfirmPassword)
            {
                return BadRequest("Mật khẩu mới và xác nhận mật khẩu không khớp.");
            }

            string hashedPassword = ConvertToSHA1(request.NewPassword);

            var account = _context.Accounts.FirstOrDefault(a => a.AccountId == customer.AccountId);
            if (account == null)
            {
                return BadRequest("Tài khoản không hợp lệ.");
            }

            account.Password = hashedPassword;
            _context.SaveChanges();

            HttpContext.Session.Remove("otp");
            HttpContext.Session.Remove("storedEmail");

            return Ok("Mật khẩu đã được thay đổi thành công.");
        }
        private void SendOtpEmail(string email, string otp)
        {
            string smtpServer = "smtp.gmail.com";
            int smtpPort = 587;
            string smtpUsername = "gearupcc0205@gmail.com"; // email nguoi gui 
            string smtpPassword = "irtz vhcw rapj pycp"; // mat khau ung dung cua email nguoi gui

            var message = new MimeMessage();
            message.From.Add(new MailboxAddress("FOOD RESTAURANT", smtpUsername));
            message.To.Add(new MailboxAddress("", email));
            message.Subject = "OTP Confirmation";
            message.Body = new TextPart("plain")
            {
                Text = $"Mã xác nhận của bạn là: {otp}"
            };
            using (var client = new MailKit.Net.Smtp.SmtpClient())
            {
                client.Connect(smtpServer, smtpPort, MailKit.Security.SecureSocketOptions.StartTls);
                client.Authenticate(smtpUsername, smtpPassword);
                client.Send(message);
                client.Disconnect(true);
            }
        }
    }
}
