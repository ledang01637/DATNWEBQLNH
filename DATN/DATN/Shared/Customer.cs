using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

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

        [JsonIgnore]
        public virtual Account Accounts { get; set; }

        [JsonIgnore]
        public virtual ICollection<RewardPointe> RewardPoints { get; set; }

        [JsonIgnore]
        public virtual ICollection<Order> Orders { get; set; }

        [JsonIgnore]
        public virtual ICollection<CustomerVoucher> CustomerVouchers { get; set; }
    }
}
