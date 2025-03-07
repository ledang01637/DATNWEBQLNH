﻿using System;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

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

        public virtual Customer Customers { get; set; }

        [JsonIgnore]
        public virtual Order Orders { get; set; }
    }
}
