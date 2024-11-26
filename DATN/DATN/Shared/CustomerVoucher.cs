using System;
using System.ComponentModel.DataAnnotations;

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
        public bool IsDeleted { get; set; }

        public virtual Order Order { get; set; }
        public virtual Voucher Vouchers { get; set; }
        public virtual Customer Customers { get; set; }


    }
}
