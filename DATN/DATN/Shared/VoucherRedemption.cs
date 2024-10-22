using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DATN.Shared
{
    public class VoucherRedemption
    {
        [Key]
        public int UseVoucherId { get; set; }
        public int CustomerId { get; set; }
        public DateTime OrderDate { get; set; }

        public Customer Customers { get; set; }
    }
}
