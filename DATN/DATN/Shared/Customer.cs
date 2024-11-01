using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace DATN.Shared
{
    public class Customer
    {
        [Key]
        public int CustomerId { get; set; }

        public string CustomerName { get; set; }
        public string PhoneNumber { get; set; }
        public string Address { get; set; }
        public string Email { get; set; }
        public int AccountId { get; set; }
        public int TotalRewardPoint { get; set; }
        public bool IsDeleted { get; set; }

        public virtual Account Accounts { get; set; }
        public virtual ICollection<RewardPointe> RewardPoints { get; set; }
        public virtual ICollection<Order> Orders { get; set; }
        public virtual ICollection<CustomerVoucher> CustomerVouchers { get; set; }
    }
}
