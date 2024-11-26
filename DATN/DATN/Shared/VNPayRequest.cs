using DATN.Shared;
using System;

namespace DATN.Shared
{
    public class VNPayRequest
    {
        public int OrderId { get; set; }
        public string FullName { get; set; }
        public string Description { get; set; }
        public decimal Amount { get; set; }
        public DateTime CreatedDate { get; set; }

    }
}
