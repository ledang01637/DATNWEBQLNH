using Microsoft.AspNetCore.Mvc;
using DATN.Shared;
using Microsoft.AspNetCore.Http;
using System.Collections.Generic;
using DATN.Server.Service;

namespace DATN.Server.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CustomerVoucherController : ControllerBase
    {
        private readonly CustomerVoucherService _CustomerVoucherService;

        public CustomerVoucherController(CustomerVoucherService _customerVoucher)
        {
            _CustomerVoucherService = _customerVoucher;
        }

        [HttpGet("GetCustomerVoucher")]
        public List<CustomerVoucher> GetCustomerVoucher()
        {
            return _CustomerVoucherService.GetCustomerVoucher();
        }

        [HttpPost("AddCustomerVoucher")]
        public CustomerVoucher AddCustomerVoucher(CustomerVoucher CustomerVoucher)
        {
            return _CustomerVoucherService.AddCustomerVoucher(new CustomerVoucher
            {
                CustomerId = CustomerVoucher.CustomerId,
                VoucherId = CustomerVoucher.VoucherId,
                IsUsed = CustomerVoucher.IsUsed,
                Status = CustomerVoucher.Status,
                RedeemDate = CustomerVoucher.RedeemDate,
                ExpirationDate = CustomerVoucher.ExpirationDate,

            });
        }

        [HttpGet("{id}")]
        public ActionResult<CustomerVoucher> GetIdCustomerVoucher(int id)
        {
            if (id == 0)
            {
                return BadRequest("Value must be...");

            }
            return Ok(_CustomerVoucherService.GetIdCustomerVoucher(id));
        }


        [HttpDelete("{id}")]
        public IActionResult DeleteVoucher(int id)
        {
            var deleted = _CustomerVoucherService.DeleteCustomerVoucher(id);
            if (deleted == null)
            {
                return NotFound("CustomerVoucher not found");
            }

            return Ok(deleted);
        }

        [HttpPut("{id}")]
        public IActionResult UpdateCustomerVoucher(int id, [FromBody] CustomerVoucher updatedCustomerVoucher)
        {
            var updated = _CustomerVoucherService.UpdateCustomerVoucher(id, updatedCustomerVoucher);
            if (updated == null)
            {
                return NotFound("CustomerVoucher not found");
            }

            return Ok(updated);
        }
    }
}
