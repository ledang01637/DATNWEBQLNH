using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.IO;
using System.Threading.Tasks;
using System;

namespace DATN.Server.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class FeeController : ControllerBase
    {
        private readonly string filePath = Path.Combine(Directory.GetCurrentDirectory(), "Data", "fee.txt");


        [HttpPost("post-fee")]
        public async Task<IActionResult> PostFee([FromBody] decimal fee)
        {
            if(fee < 0)
            {
                return BadRequest("Phí không hợp lệ");
            }

            await System.IO.File.WriteAllTextAsync(filePath, fee.ToString());

            return Ok("Cập nhật phí đặt bàn thành công");
        }

        [HttpGet("get-fee")]
        public async Task<IActionResult> GetFee()
        {
            try
            {
                if (!System.IO.File.Exists(filePath))
                {
                    return NotFound("Tệp tin không tồn tại.");
                }
                var fee = await System.IO.File.ReadAllTextAsync(filePath);

                return Ok(fee);
            }
            catch (Exception ex)
            {
                return StatusCode(500, "Lỗi khi đọc tệp: " + ex.Message);
            }
        }
    }
}
