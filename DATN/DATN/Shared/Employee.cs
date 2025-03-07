using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace DATN.Shared
{
    public class Employee
    {
        [Key]
        public int EmployeeId { get; set; }
        public string EmployeeName { get; set; }
        public string Address { get; set; }
        public string PhoneNumber { get; set; }
        public string Email { get; set; }
        public string Position { get; set; }
        public DateTime HireDate { get; set; }
        public decimal Salary { get; set; }
        public bool IsDeleted { get; set; }
        public int AccountId { get; set; }

        public virtual Account Accounts { get; set; }
        public virtual ICollection<EmployeeShifte> EmployeeShiftes { get; set; }
        public virtual ICollection<Order> Orders { get; set; }
    }
}
