using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
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
        public bool IsAcctive { get; set; }
        public bool IsDeleted { get; set; }

        [JsonIgnore]
        public virtual ICollection<CustomerVoucher> CustomerVouchers { get; set; }
    }
}
