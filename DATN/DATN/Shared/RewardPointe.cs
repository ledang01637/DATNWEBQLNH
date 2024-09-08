using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DATN.Shared
{
    public class RewardPointe
    {
        [Key]
        public int RewardPointId { get; set; }
        public int CustomerId { get; set; }
        public int RewardPoint { get; set; }
        public DateTime UpdateDate { get; set; }
        public bool IsDeleted { get; set; }
        public int OrderId { get; set; }

        public Customer Customers { get; set; }
        public Order Orders { get; set; }
    }
}
