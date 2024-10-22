using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DATN.Shared
{
    public class Shifte
    {
        [Key]
        public int Shifte_Id { get; set; }
        public string Shifte_Name { get; set; }
        public TimeSpan StartTime { get; set; }
        public TimeSpan EndTime { get; set; }
        public bool IsDeleted { get; set; }

        public ICollection<EmployeeShifte> EmployeeShiftes { get; set; }
    }
}
