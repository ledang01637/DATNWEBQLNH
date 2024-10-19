using Microsoft.AspNetCore.Mvc;
using DATN.Server.Service;
using DATN.Server.Hash;
using System.Threading.Tasks;
using System.Linq;
using System;
using System.Security.Cryptography;
using System.Text;

namespace DATN.Server.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class NetworkController : ControllerBase
    {
        private readonly NetworkService _networkService;
        private readonly FileEncryptionService _fileEncryptionService;

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
                encryptedContent = await System.IO.File.ReadAllTextAsync("Data/valid_wifi.txt");
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

        [HttpPost("post-wifi-ip")]
        public async Task<IActionResult> PostWifiIpAddressAsync([FromBody] string IP)
        {
            if (string.IsNullOrWhiteSpace(IP))
            {
                return BadRequest("IP không hợp lệ.");
            }

            string encryptedIp = ComputeMD5Hash(IP);

            string existingContent = await System.IO.File.ReadAllTextAsync("Data/valid_wifi.txt");

            var ipList = existingContent.Split(new[] { ';' }, StringSplitOptions.RemoveEmptyEntries);

            if (ipList.Contains(encryptedIp))
            {
                return BadRequest("IP đã tồn tại.");
            }

            existingContent += ";" + encryptedIp;

            await System.IO.File.WriteAllTextAsync("Data/valid_wifi.txt", existingContent);

            return Ok("IP đã được thêm thành công.");
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
                existingContent = await System.IO.File.ReadAllTextAsync("Data/valid_wifi.txt");
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
            await System.IO.File.WriteAllTextAsync("Data/valid_wifi.txt", updatedContent);

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
