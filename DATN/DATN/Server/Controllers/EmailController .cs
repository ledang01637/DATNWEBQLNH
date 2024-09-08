using MailKit.Net.Smtp;
using Microsoft.AspNetCore.Mvc;
using MimeKit;
using System.Threading.Tasks;

namespace DATN.Server.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class EmailController : ControllerBase
    {
        [HttpPost("send")]
        public async Task<IActionResult> SendEmail([FromBody] EmailRequest emailRequest)
        {
            var message = new MimeMessage();
            message.From.Add(new MailboxAddress("Your App Name", "your-email@example.com"));
            message.To.Add(new MailboxAddress("", emailRequest.ToEmail));
            message.Subject = emailRequest.Subject;

            message.Body = new TextPart("plain")
            {
                Text = emailRequest.Body
            };

            using (var client = new SmtpClient())
            {
                await client.ConnectAsync("smtp.your-email-provider.com", 587, false);
                await client.AuthenticateAsync("your-email@example.com", "your-email-password");
                await client.SendAsync(message);
                await client.DisconnectAsync(true);
            }

            return Ok();
        }
    }

    public class EmailRequest
    {
        public string ToEmail { get; set; }
        public string Subject { get; set; }
        public string Body { get; set; }
    }
}
