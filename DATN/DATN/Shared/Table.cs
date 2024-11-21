using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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

        public Floor Floors { get; set; }
        public ICollection<Order> Orders { get; set; }
        public ICollection<Reservation> Reservations { get; set; }
    }
}
