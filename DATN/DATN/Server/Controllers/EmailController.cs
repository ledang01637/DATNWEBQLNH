using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Net.Mail;
using System.Net;
using System.Threading.Tasks;
using System;
using DATN.Shared;

namespace DATN.Server.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class EmailController : ControllerBase
    {
        [HttpPost("send-email")]
        public async Task<IActionResult> SendEmail([FromBody] EmailRequest emailRequest)
        {
            string from = "danglhpc06254@fpt.edu.vn";
            string password = "ylbrrghnetlzudgm";

            try
            {
                using (MailMessage mail = new MailMessage())
                {
                    mail.From = new MailAddress(from);
                    mail.To.Add(emailRequest.To);
                    mail.Subject = emailRequest.Subject;
                    mail.Body = emailRequest.Body;

                    using (SmtpClient smtpClient = new SmtpClient("smtp.gmail.com", 587))
                    {
                        smtpClient.Credentials = new NetworkCredential(from, password);
                        smtpClient.EnableSsl = true;
                        await smtpClient.SendMailAsync(mail);
                    }
                }

                return Ok(new { success = true, message = "Email sent successfully!" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { success = false, message = ex.Message });
            }
        }

    }
}
