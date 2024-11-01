using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

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
        public string ProductDescription { get; set; }
        public byte[] ProductImage { get; set; }
        public bool IsDeleted { get; set; }

        public virtual Category Categories { get; set; }
        public virtual Unit Units { get; set; }
        public virtual ICollection<MenuItem> MenuItems { get; set; }

        [JsonIgnore]
        public virtual ICollection<OrderItem> OrderItems { get; set; }
    }
}
