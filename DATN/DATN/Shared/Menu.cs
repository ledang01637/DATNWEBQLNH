using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace DATN.Shared
{
    public class Menu
    {
        [Key]
        public int MenuId { get; set; }
        public string MenuName { get; set; }
        public string MenuDescription { get; set; }
        public decimal PriceCombo { get; set; }
        public byte[] MenuImage { get; set; }
        public bool IsDeleted { get; set; }

        public virtual ICollection<MenuItem> MenuItems { get; set; }

    }
}
