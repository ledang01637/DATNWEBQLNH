using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
        public bool IsDeleted { get; set; }
        public int AccountId { get; set; }
        public decimal TotalRewardPoint { get; set; }

        public Account Accounts { get; set; }
        public ICollection<Order> Orders { get; set; }
        public RewardPointe RewardPoints { get; set; }
        public ICollection<CustomerVoucher> CustomerVouchers { get; set; }
    }
}
