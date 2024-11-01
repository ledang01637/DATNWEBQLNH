
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace DATN.Shared
{
    public class Unit
    {
        [Key]
        public int UnitId { get; set; }

        public string UnitName { get; set; }
        public string UnitDescription { get; set; }
        public bool IsDeleted { get; set; }

        public virtual ICollection<Product> Products { get; set; }
    }
}
