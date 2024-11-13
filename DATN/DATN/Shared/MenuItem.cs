using System.ComponentModel.DataAnnotations;

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
