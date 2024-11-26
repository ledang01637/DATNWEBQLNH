using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DATN.Shared
{
    public class Transaction
    {
        [Key]
        public int TransactionId {  get; set; }
        public int ReservationId { get; set; }
        public decimal Amount {  get; set; }
        public string PaymentStatus {  get; set; }
        public DateTime PaymentDate {  get; set; }
        public virtual Reservation Reservation { get; set; }
    }
}
