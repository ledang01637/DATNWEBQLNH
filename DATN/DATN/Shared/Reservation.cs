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

        [Required(ErrorMessage = "Họ tên không được để trống")]
        public string CustomerName { get; set; }

        [Required(ErrorMessage = "Số điện thoại không được để trống")]
        [RegularExpression(@"^((\+84)|0)(3|5|7|8|9)[0-9]{8}$", ErrorMessage = "Số điện thoại phải từ 10 đến 11 ký tự và bắt đầu bằng 0 hoặc +84")]
        public string CustomerPhone { get; set; }

        [Required(ErrorMessage = "Email không được để trống")]
        [EmailAddress(ErrorMessage = "Email không hợp lệ")]
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
