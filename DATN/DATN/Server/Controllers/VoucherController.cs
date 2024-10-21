using Microsoft.AspNetCore.Mvc;
using DATN.Shared;
using Microsoft.AspNetCore.Http;
using System.Collections.Generic;
using DATN.Server.Service;

namespace DATN.Server.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class VoucherController : ControllerBase
    {
        private readonly VoucherService _VoucherService;

        public VoucherController(VoucherService _voucher)
        {
            _VoucherService = _voucher;
        }

        [HttpGet("GetVoucher")]
        public List<Voucher> GetVoucher()
        {
            return _VoucherService.GetVoucher();
        }

        [HttpPost("AddVoucher")]
        public Voucher AddVoucher(Voucher Voucher)
        {
            return _VoucherService.AddVoucher(new Voucher
            {
                VoucherCode = Voucher.VoucherCode,
                PointRequired = Voucher.PointRequired,
                DiscountValue = Voucher.DiscountValue,
                ExpriationDate = Voucher.ExpriationDate,
                IsAcctive = Voucher.IsAcctive,
                IsDeleted = Voucher.IsDeleted

            });
        }

        [HttpGet("{id}")]
        public ActionResult<Voucher> GetIdVoucher(int id)
        {
            if (id == 0)
            {
                return BadRequest("Value must be...");

            }
            return Ok(_VoucherService.GetIdVoucher(id));
        }


        [HttpDelete("{id}")]
        public IActionResult DeleteVoucher(int id)
        {
            var deleted = _VoucherService.DeleteVoucher(id);
            if (deleted == null)
            {
                return NotFound("Voucher not found");
            }

            return Ok(deleted);
        }

        [HttpPut("{id}")]
        public IActionResult UpdateVoucher(int id, [FromBody] Voucher updatedVoucher)
        {
            var updated = _VoucherService.UpdateVoucher(id, updatedVoucher);
            if (updated == null)
            {
                return NotFound("Voucher not found");
            }

            return Ok(updated);
        }
    }
}
