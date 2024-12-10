using Microsoft.AspNetCore.Mvc;
using DATN.Server.Service;
using DATN.Server.Hash;
using System.Threading.Tasks;
using System.Linq;
using System;
using System.Security.Cryptography;
using System.Text;
using System.IO;
using Microsoft.AspNetCore.Http;

namespace DATN.Server.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class NetworkController : ControllerBase
    {
        private readonly NetworkService _networkService;
        private readonly FileEncryptionService _fileEncryptionService;
        private readonly string filePath = Path.Combine(Directory.GetCurrentDirectory(), "Data", "valid_wifi.txt");
        public NetworkController(NetworkService networkService, FileEncryptionService fileEncryptionService)
        {
            _networkService = networkService;
            _fileEncryptionService = fileEncryptionService;
        }

        [HttpGet("wifi-ip")]
        public async Task<IActionResult> GetWifiIpAddressAsync()
        {
            var remoteIpAddress = GetClientIP(HttpContext);

            string encryptedContent;

            try
            {
                encryptedContent = await System.IO.File.ReadAllTextAsync(filePath);
            }
            catch (Exception ex)
            {
                return StatusCode(500, "Lỗi khi đọc tệp: " + ex.Message);
            }

            if (string.IsNullOrEmpty(encryptedContent))
            {
                return BadRequest("Nội dung Wifi không hợp lệ.");
            }
            var ipList = encryptedContent.Split(new[] { ';' }, StringSplitOptions.RemoveEmptyEntries);

            string hashedRemoteIp = ComputeSHA256Hash(remoteIpAddress);

            if (ipList.Contains(hashedRemoteIp))
            {
                return Ok(remoteIpAddress);
            }

            return BadRequest("Địa chỉ IP không hợp lệ.");
        }

        //Local
        [HttpGet("get-ip-local")]
        public IActionResult GetIPWifi()
        {
            var ip = _networkService.GetWifiIPAddress();
            if (!string.IsNullOrEmpty(ip))
            {
                return Ok(ip);
            }
            return NotFound("Không tìm thấy IP");
        }

        //Host
        [HttpGet("get-ip-host")]
        public IActionResult GetIPWifiHost()
        {
            var ip = GetClientIP(HttpContext);

            if (!string.IsNullOrEmpty(ip))
            {
                return Ok(ip);
            }
            return NotFound("Không tìm thấy IP");
        }

        private static string GetClientIP(HttpContext context)
        {
            string ip = context.Request.Headers["CF-Connecting-IP"].FirstOrDefault();

            if (string.IsNullOrEmpty(ip))
            {
                ip = context.Request.Headers["X-Forwarded-For"].FirstOrDefault();
            }

            if (string.IsNullOrEmpty(ip))
            {
                ip = context.Connection.RemoteIpAddress?.ToString();
            }

            if (ip == "::1")
            {
                ip = "127.0.0.1";
            }

            return ip;
        }

        [HttpPost("post-wifi-ip")]
        public async Task<IActionResult> PostWifiIpAddressAsync([FromBody] string IP)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(IP))
                {
                    return BadRequest("IP không hợp lệ.");
                }

                string encryptedIp = ComputeSHA256Hash(IP);

                string existingContent = await System.IO.File.ReadAllTextAsync(filePath);

                var ipList = existingContent.Split(new[] { ';' }, StringSplitOptions.RemoveEmptyEntries);

                if (ipList.Contains(encryptedIp))
                {
                    return BadRequest("IP đã tồn tại.");
                }
                existingContent += ";" + encryptedIp;

                await System.IO.File.WriteAllTextAsync(filePath, existingContent);

                return Ok("Thêm IP thành công.");
            }
            catch
            {
                return StatusCode(500, "Lỗi server.");
            }
        }

        [HttpPost("remove-wifi-ip")]
        public async Task<IActionResult> RemoveWifiIpAddressAsync([FromBody] string IP)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(IP))
                {
                    return BadRequest("IP không hợp lệ.");
                }

                string encryptedIp = ComputeSHA256Hash(IP);

                var existingContent = await System.IO.File.ReadAllTextAsync(filePath);

                var ipList = existingContent.Split(new[] { ';' }, StringSplitOptions.RemoveEmptyEntries).ToList();

                if (!ipList.Contains(encryptedIp))
                {
                    return BadRequest("IP không tồn tại.");
                }
                ipList.Remove(encryptedIp);
                string updatedContent = string.Join(";", ipList);
                await System.IO.File.WriteAllTextAsync(filePath, updatedContent);

                return Ok("Xóa thành công.");

            }
            catch
            {
                return StatusCode(500, "Lỗi đọc tệp");
            }

        }

        private static string ComputeSHA256Hash(string rawData)
        {
            using var sha256 = SHA256.Create();
            byte[] bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(rawData));
            return Convert.ToBase64String(bytes);
        }
    }
}
