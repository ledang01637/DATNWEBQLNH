using DATN.Shared;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace DATN.Server.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class DataCustomerController : ControllerBase
    {
        private readonly string FilePath = Path.Combine(Directory.GetCurrentDirectory(), "Data", "data_customer.txt");

        [HttpPost("save")]
        public async Task<IActionResult> PostCustomerDataAsync([FromBody] DataCustomer dataCustomer)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest("Dữ liệu không hợp lệ.");
                }

                string encryptedDataCustomer = EncryptDataCustomer(dataCustomer);

                if (!System.IO.File.Exists(FilePath))
                {
                    Directory.CreateDirectory(Path.GetDirectoryName(FilePath));
                }

                var existingData = System.IO.File.Exists(FilePath)
                    ? System.IO.File.ReadAllLines(FilePath).ToList()
                    : new List<string>();

                var dataCustomerList = existingData
                    .Where(line => !string.IsNullOrWhiteSpace(line))
                    .Select(DecryptDataCustomer)
                    .ToList();

                if (string.IsNullOrWhiteSpace(dataCustomer.PhoneNumber))
                {
                    return BadRequest("Số điện thoại không được để trống.");
                }

                if (dataCustomerList.Any(c => c.PhoneNumber == dataCustomer.PhoneNumber))
                {
                    return BadRequest("Số điện thoại đã tồn tại.");
                }

                await System.IO.File.AppendAllTextAsync(FilePath, encryptedDataCustomer + Environment.NewLine);

                var data = DecryptDataCustomer(encryptedDataCustomer);

                return Ok(data);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Lỗi server: {ex.Message}");
            }
        }



        [HttpGet("list")]
        public IActionResult GetAllDataCustomers()
        {
            try
            {
                if (!System.IO.File.Exists(FilePath))
                {
                    return Ok(new List<DataCustomer>());
                }

                var dataCustomerList = new List<DataCustomer>();

                foreach (var line in System.IO.File.ReadAllLines(FilePath))
                {
                    try
                    {
                        var customer = DecryptDataCustomer(line);
                        dataCustomerList.Add(customer);
                    }
                    catch
                    {
                        return StatusCode(500, "Lỗi server");
                    }
                }

                return Ok(dataCustomerList);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Lỗi server: {ex.Message}");
            }
        }

        [HttpGet("getbyphone")]
        public IActionResult GetCustomerByPhone([FromQuery] string phone, [FromQuery] string name)
        {
            try
            {
                if (!System.IO.File.Exists(FilePath))
                {
                    return NotFound("File dữ liệu không tồn tại.");
                }

                var data = System.IO.File.ReadAllLines(FilePath);

                if(data == null || !data.Any() || data.Length == 0 || data.All(string.IsNullOrWhiteSpace))
                {
                    return null;
                }

                foreach (var line in data)
                {
                    try
                    {
                        var customer = DecryptDataCustomer(line);
                        if (customer.PhoneNumber == phone && customer.Name == name)
                        {
                            return Ok(customer);
                        }
                    }
                    catch
                    {
                        continue;
                    }
                }

                return null;
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Lỗi server: {ex.Message}");
            }
        }


        private static string EncryptDataCustomer(DataCustomer dataCustomer)
        {
            var jsonData = System.Text.Json.JsonSerializer.Serialize(dataCustomer);
            return Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(jsonData));
        }

        private DataCustomer DecryptDataCustomer(string encryptedData)
        {
            var jsonData = System.Text.Encoding.UTF8.GetString(Convert.FromBase64String(encryptedData));
            return System.Text.Json.JsonSerializer.Deserialize<DataCustomer>(jsonData);
        }
    }

}
