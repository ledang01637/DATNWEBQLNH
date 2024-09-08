using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DATN.Shared
{
    public class Product
    {
        [Key]
        public int ProductId { get; set; }
        public int UnitId { get; set; }
        public string ProductName { get; set; }
        public decimal Price { get; set; }
        public int CategoryId { get; set; }
        public string? ProductDescripntion { get; set; }
        public string ProductImage { get; set; }
        public bool IsDelete { get; set; }

        public Category Categories { get; set; }
        public Unit units { get; set; }
        public ICollection<MenuItem> MenuItems { get; set; }
        public ICollection<OrderItem> OrderItems { get; set; }
    }
}
