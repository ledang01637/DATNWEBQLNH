using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DATN.Shared
{
    public class User
    {
        public int Id { get; set; }

        public int RoleId { get; set; }

        public DateTime CreatedAt { get; set; }

        public string AccountType { get; set; }

        [Required(ErrorMessage = "Tên người dùng là bắt buộc")]
        public string Username { get; set; }

        [Required(ErrorMessage = "Phone là bắt buộc")]
        [RegularExpression(@"^(0[3|5|7|8|9])+([0-9]{8})$", ErrorMessage = "Không đúng định dạng số điện thoại")]
        public string PhoneNumber { get; set; }

        [Required(ErrorMessage = "Email là bắt buộc")]
        [RegularExpression(@"^[a-zA-Z0-9_.+-]+@[a-zA-Z0-9-]{3,}\.[a-zA-Z0-9-.]+$", ErrorMessage = "Email không hợp lệ")]
        public string Email { get; set; }

        [Required(ErrorMessage = "Mật khẩu là bắt buộc")]
        public string Password { get; set; }

        [Required(ErrorMessage = "Xác nhận mật khẩu là bắt buộc")]
        [Compare("Password", ErrorMessage = "Mật khẩu và xác nhận mật khẩu không khớp")]
        public string ConfirmPassword { get; set; }
    }
}
