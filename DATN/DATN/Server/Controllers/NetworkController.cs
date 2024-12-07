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
        private string filePath = Path.Combine(Directory.GetCurrentDirectory(), "Data", "valid_wifi.txt");
        public NetworkController(NetworkService networkService, FileEncryptionService fileEncryptionService)
        {
            _networkService = networkService;
            _fileEncryptionService = fileEncryptionService;
        }

        [HttpGet("wifi-ip")]
        public async Task<IActionResult> GetWifiIpAddressAsync()
        {
            var remoteIpAddress = _networkService.GetWifiIPAddress();

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

            string hashedRemoteIp = ComputeMD5Hash(remoteIpAddress);

            if (ipList.Contains(hashedRemoteIp))
            {
                return Ok(remoteIpAddress);
            }

            return BadRequest("Địa chỉ IP không hợp lệ.");
        }

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

                return Ok("IP đã được thêm thành công.");
            }
            catch
            {
                return StatusCode(StatusCodes.Status500InternalServerError);
            }
        }

        // Hàm mã hóa SHA256
        private string ComputeSHA256Hash(string rawData)
        {
            using (var sha256 = System.Security.Cryptography.SHA256.Create())
            {
                byte[] bytes = sha256.ComputeHash(System.Text.Encoding.UTF8.GetBytes(rawData));
                return Convert.ToBase64String(bytes);
            }
        }


        [HttpPost("remove-wifi-ip")]
        public async Task<IActionResult> RemoveWifiIpAddressAsync([FromBody] string IP)
        {
            if (string.IsNullOrWhiteSpace(IP))
            {
                return BadRequest("IP không hợp lệ.");
            }

            string encryptedIp = ComputeMD5Hash(IP);
            string existingContent;

            try
            {
                existingContent = await System.IO.File.ReadAllTextAsync(filePath);
            }
            catch (Exception ex)
            {
                return StatusCode(500, "Lỗi khi đọc tệp: " + ex.Message);
            }

            var ipList = existingContent.Split(new[] { ';' }, StringSplitOptions.RemoveEmptyEntries).ToList();

            if (!ipList.Contains(encryptedIp))
            {
                return BadRequest("IP không tồn tại.");
            }
            ipList.Remove(encryptedIp);
            string updatedContent = string.Join(";", ipList);
            await System.IO.File.WriteAllTextAsync(filePath, updatedContent);

            return Ok("IP đã được xóa thành công.");
        }


        public string ComputeMD5Hash(string input)
        {
            using (MD5 md5 = MD5.Create())
            {
                byte[] inputBytes = Encoding.UTF8.GetBytes(input);
                byte[] hashBytes = md5.ComputeHash(inputBytes);

                StringBuilder sb = new StringBuilder();
                foreach (byte b in hashBytes)
                {
                    sb.Append(b.ToString("x2"));
                }
                return sb.ToString();
            }
        }
    }
}
