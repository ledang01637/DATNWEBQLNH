using DATN.Shared;
using Microsoft.AspNetCore.Http;

namespace DATN.Server.Payment.ServicePayment
{
    public interface IVNPayService
    {
        string CreatePaymentUrl(HttpContext httpContext, VNPayRequest vNPayRequest);
        VNPayResponse PaymentResponse(IQueryCollection queryCollection);
    }
}
