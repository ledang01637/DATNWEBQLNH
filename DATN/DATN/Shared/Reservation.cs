using System;
using System.ComponentModel.DataAnnotations;

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
        public bool IsPayment {  get; set; }
        public decimal DepositPayment {  get; set; }
        public string PaymentMethod {  get; set; }
        public bool IsDeleted {  get; set; }

        public virtual Table Tables { get; set; }
    }
}
