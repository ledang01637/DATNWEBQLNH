﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DATN.Shared
{
    public class Voucher
    {
        [Key]
        public int VoucherId { get; set; }
        public string VoucherCode { get; set; }
        public int PointRequired { get; set; }
        public decimal DiscountValue { get; set; }
        public DateTime ExpriationDate {  get; set; }
        public bool IsActive { get; set; }

        public ICollection<CustomerVoucher> CustomerVouchers { get; set; }
    }
}
