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
        public int TableId { get; set; }
        public string CustomerName { get; set; }
        public string CustomerPhone { get; set; }
        public DateTime ReservationDate { get; set; }
        public int NumberGuest { get; set; }
        public int NumberSeat {  get; set; }
        public bool Is_Payment {  get; set; }
        public decimal DepositPayment {  get; set; }
        public string PaymentMethod {  get; set; }
        public bool IsDeleted {  get; set; }

        public virtual Table Tables { get; set; }
    }
}
