using DATN.Server.Service;
using Microsoft.AspNetCore.Mvc;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

[ApiController]
[Route("api/[controller]")]
public class VoiceController : ControllerBase
{
    private readonly string FilePath = "Data/message-res.txt";

    [HttpPost("post-message")]
    public async Task<IActionResult> PostMessage([FromBody] string message)
    {
        if (string.IsNullOrWhiteSpace(message))
        {
            return BadRequest("Message không hợp lệ.");
        }

        await System.IO.File.WriteAllTextAsync(FilePath, message);

        return Ok("Message đã được thêm thành công.");
    }

    [HttpGet("get-message")]
    public async Task<IActionResult> GetMessage()
    {
        try
        {
            if (!System.IO.File.Exists(FilePath))
            {
                return NotFound("Tệp tin không tồn tại.");
            }
            var message = await System.IO.File.ReadAllTextAsync("Data/message-res.txt");
            return Ok(message);
        }
        catch (Exception ex)
        {
            return StatusCode(500, "Lỗi khi đọc tệp: " + ex.Message);
        }
    }
}
