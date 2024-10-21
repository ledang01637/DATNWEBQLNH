using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DATN.Shared
{
    public class MenuItem
    {
        [Key]
        public int MenuItemId { get; set; }
        public int MenuId { get; set; }
        public int ProductId { get; set; }
        public bool IsDeleted { get; set; }

        public virtual Product Products { get; set; }
        public virtual Menu Menus { get; set; }
    }
}
