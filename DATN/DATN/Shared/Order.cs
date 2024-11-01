using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace DATN.Shared
{
    public class Order
    {
        [Key]
        public int OrderId { get; set; }
        public int TableId { get; set; }
        public int EmployeeId {  get; set; }
        public DateTime CreateDate { get; set; }
        public decimal TotalAmount { get; set; }
        public string Status { get; set; }
        public int CustomerId { get; set; }
        public string PaymentMethod { get; set; }
        public string Note { get; set; }
        public int? CustomerVoucherId { get; set; }
        public bool IsDeleted { get; set; }

        public virtual Customer Customers { get; set; }
        public virtual Table Tables { get; set; }
        public virtual ICollection<OrderItem> OrderItems { get; set; }
        public virtual CustomerVoucher CustomerVouchers { get; set; }
        public virtual Employee Employee { get; set; }
        public virtual RewardPointe RewardPointes { get; set; }
    }
}
