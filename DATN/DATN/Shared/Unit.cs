using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DATN.Shared
{
    public class Unit
    {
        [Key]
        public int UnitId { get; set; }

        public string UnitName { get; set; }
        public string UnitDescription { get; set; }

        public ICollection<Product> Products { get; set; }
    }
}
