﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace DATN.Shared
{
    public class Account
    {
        [Key]
        public int AccountId { get; set; }
        public string AccountType { get; set; }
        public DateTime CreateDate { get; set; }
        public DateTime UpdateDate { get; set; }
        public string Email { get; set; }
        public string Password { get; set; }
        public bool IsActive { get; set; }
        public bool IsDeleted { get; set; }

        public virtual ICollection<RoleAccount> RoleAccounts { get; set; }
        public virtual ICollection<Message> Messages { get; set; }
        public virtual Employee Employees { get; set; }

        [JsonIgnore]
        public virtual Customer Customers { get; set; }

    }
}
