using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
namespace DATN.Shared
{
    public class Shifte
    {
        [Key]
        public int ShifteId { get; set; }
        public string ShifteName { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public bool IsDeleted { get; set; }

        public virtual ICollection<EmployeeShifte> EmployeeShiftes { get; set; }
    }
}
