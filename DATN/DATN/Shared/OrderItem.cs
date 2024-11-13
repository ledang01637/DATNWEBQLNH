using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace DATN.Shared
{
    public class OrderItem
    {
        [Key]
        public int OrderItemId { get; set; }
        public int OrderId { get; set; }
        public int ProductId { get; set; }
        public int Quantity { get; set; }
        public decimal Price { get; set; }
        public decimal TotalPrice { get; set; }
        public bool IsDeleted { get; set; }

        [JsonIgnore]
        public virtual Order Orders { get; set; }
        public virtual Product Products { get; set; }

    }
}
