using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace DATN.Shared
{
    public class Customer
    {
        [Key]
        public int CustomerId { get; set; }

        [Required(ErrorMessage = "Họ tên không được để trống")]
        public string CustomerName { get; set; }

        [Required(ErrorMessage = "Số điện thoại không được để trống")]
        [RegularExpression(@"^((\+84)|0)(3|5|7|8|9)[0-9]{8}$", ErrorMessage = "Số điện thoại phải từ 10 đến 11 ký tự và bắt đầu bằng 0 hoặc +84")]
        public string PhoneNumber { get; set; }

        [Required(ErrorMessage = "Địa chỉ không được để trống")]
        public string Address { get; set; }

        [Required(ErrorMessage = "Email không được để trống")]
        [EmailAddress(ErrorMessage = "Email không hợp lệ")]
        public string Email { get; set; }
        public int AccountId { get; set; }
        public int TotalRewardPoint { get; set; }
        public bool IsDeleted { get; set; }
        public virtual Account Accounts { get; set; }

        [JsonIgnore]
        public virtual ICollection<RewardPointe> RewardPoints { get; set; }

        [JsonIgnore]
        public virtual ICollection<Order> Orders { get; set; }

        [JsonIgnore]
        public virtual ICollection<CustomerVoucher> CustomerVouchers { get; set; }
    }
}
