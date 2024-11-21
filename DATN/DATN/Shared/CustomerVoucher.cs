using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DATN.Shared
{
    public class CustomerVoucher
    {
        [Key]
        public int CustomerVoucherId { get; set; }
        public int CustomerId { get; set; }
        public int VoucherId {  get; set; }
        public bool IsUsed { get; set; }
        public string Status { get; set; }
        public DateTime RedeemDate { get; set; }
        public DateTime ExpirationDate { get; set; }

        public Order Order { get; set; }
        public Voucher Vouchers { get; set; }
        public Customer Customers { get; set; }


    }
}
