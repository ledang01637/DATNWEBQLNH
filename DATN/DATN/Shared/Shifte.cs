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
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public bool IsDeleted { get; set; }

        public virtual ICollection<EmployeeShifte> EmployeeShiftes { get; set; }
    }
}
