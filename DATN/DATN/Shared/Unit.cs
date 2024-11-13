
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace DATN.Shared
{
    public class Unit
    {
        [Key]
        public int UnitId { get; set; }

        public string UnitName { get; set; }
        public string UnitDescription { get; set; }
        public bool IsDeleted { get; set; }

        [JsonIgnore]
        public virtual ICollection<Product> Products { get; set; }
    }
}
