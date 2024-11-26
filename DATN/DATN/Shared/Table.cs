
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace DATN.Shared
{
    public class Table
    {
        [Key]
        public int TableId { get; set; }
        public int FloorId { get; set; }
        public int TableNumber { get; set; }
        public string Position { get; set; }
        public int SeatingCapacity { get; set; }
        public string Status { get; set; }
        public bool IsDeleted { get; set; }

        public virtual Floor Floors { get; set; }
        public virtual ICollection<Order> Orders { get; set; }

        [JsonIgnore]
        public virtual ICollection<Reservation> Reservations { get; set; }
        public virtual ICollection<Message> Messages { get; set; }
    }
}
