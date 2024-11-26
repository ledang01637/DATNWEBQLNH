using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DATN.Shared
{
    public class EmployeeShifte
    {
        [Key]
        public int EmployeeShifteId { get; set; }
        public DateTime ShifteDay { get; set; }
        public int EmployeeId { get; set; }
        public int ShifteId { get; set; }

        public bool IsDeleted { get; set; }

        public virtual Employee Employees { get; set; }
        public virtual Shifte Shiftes { get; set; }
    }
}
