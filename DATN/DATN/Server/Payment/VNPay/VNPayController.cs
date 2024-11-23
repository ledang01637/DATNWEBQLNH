using DATN.Server.Payment.ServicePayment;
using DATN.Server.Service;
using DATN.Shared;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Linq;

namespace DATN.Server.Payment.VNPay
{
    [Route("api/[controller]")]
    [ApiController]
    public class VNPayController : ControllerBase
    {
        private readonly VNPayService _VNPayService;

        public VNPayController(VNPayService VNPayService)
        {
            _VNPayService = VNPayService;
        }

        [HttpPost("CreateUrlVNPay")]
        public ActionResult<string> CreateUrlVNPay(VNPayRequest vNPayRequest)
        {
            if (vNPayRequest == null || !ModelState.IsValid)
            {
                return BadRequest(new
                {
                    Message = "Invalid request data",
                    Errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage)
                });
            }

            try
            {
                var paymentUrl = _VNPayService.CreatePaymentUrl(HttpContext, vNPayRequest);

                return Ok(paymentUrl);

            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Message = "Internal server error", Details = ex.Message });
            }
        }  
    }
}
