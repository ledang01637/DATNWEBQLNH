using System;
using System.Collections.Generic;
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
        public string CustomerEmail { get; set; }
        public DateTime ReservationTime { get; set; }
        public int Adults { get; set; }
        public int Children { get; set; }
        public bool IsPayment {  get; set; }
        public decimal DepositPayment {  get; set; }
        public string PaymentMethod {  get; set; }
        public DateTime CreatedDate {  get; set; }
        public DateTime UpdatedDate { get; set; }
        public string CustomerNote {  get; set; }
        public string ReservationStatus {  get; set; }
        public bool IsDeleted {  get; set; }

        public virtual Table Tables { get; set; }
        public virtual ICollection<Transaction> Transactions { get; set; }
    }
}
