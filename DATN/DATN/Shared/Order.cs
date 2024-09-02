using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DATN.Shared
{
    public class Order
    {
        [Key]
        public int OrderId { get; set; }
        public int TableId { get; set; }
        public DateTime OrderDate { get; set; }
        public decimal TotalAmount { get; set; }
        public string Status { get; set; }
        public int? CustomerId { get; set; }
        public string PaymentMethod { get; set; }
        public int? CustomerVoucherId { get; set; }

        public Customer Customers { get; set; }
        public Table Tables { get; set; }
        public ICollection<OrderItem> OrderItems { get; set; }
        public ICollection<RewardPointe> RewardPointes { get; set; }
        public CustomerVoucher CustomerVouchers { get; set; }
    }
}
