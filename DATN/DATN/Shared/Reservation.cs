using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace DATN.Shared
{
    public class Reservation
    {
        [Key]
        public int ReservationId { get; set; }
        public string CustomerName { get; set; }
        public string CustomerPhone { get; set; }
        public DateTime ReservationDate { get; set; }
        public DateTime ReservationTime { get; set; }
        public int Numberguest { get; set; }
        public int CustomerId { get; set; }

        public Table Tables { get; set; }
    }
}
