using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.IO;
using System.Threading.Tasks;
using System;

namespace DATN.Server.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class VoiceController : ControllerBase
    {
        private readonly string filePath = Path.Combine(Directory.GetCurrentDirectory(), "Data", "message-res.txt");


        [HttpPost("post-message")]
        public async Task<IActionResult> PostMessage([FromBody] string message)
        {
            if (string.IsNullOrWhiteSpace(message))
            {
                return BadRequest("Message không hợp lệ.");
            }

            await System.IO.File.WriteAllTextAsync(filePath, message);

            return Ok("Message đã được thêm thành công.");
        }

        [HttpGet("get-message")]
        public async Task<IActionResult> GetMessage()
        {
            try
            {
                if (!System.IO.File.Exists(filePath))
                {
                    return NotFound("Tệp tin không tồn tại.");
                }
                var message = await System.IO.File.ReadAllTextAsync(filePath);
                return Ok(message);
            }
            catch (Exception ex)
            {
                return StatusCode(500, "Lỗi khi đọc tệp: " + ex.Message);
            }
        }
    }
}
