using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DATN.Shared
{
    public class DataCustomer
    {
        public Guid CustomerGUID { get; set; } = Guid.NewGuid();

        [Required(ErrorMessage = "Họ tên không được để trống")]
        public string Name { get; set; }

        [Required(ErrorMessage = "Số điện thoại không được để trống")]
        [RegularExpression(@"^((\+84)|0)(3|5|7|8|9)[0-9]{8}$", ErrorMessage = "Số điện thoại phải từ 10 đến 11 ký tự và bắt đầu bằng 0 hoặc +84")]
        public string PhoneNumber { get; set; }

        public List<string> OrderIDs { get; set; } = new List<string>();

        public override string ToString()
        {
            return $"{CustomerGUID}|{Name}|{PhoneNumber}|{string.Join(",", OrderIDs)}";
        }

        public static DataCustomer FromString(string data)
        {
            var parts = data.Split('|');
            return new DataCustomer
            {
                CustomerGUID = Guid.Parse(parts[0]),
                Name = parts[1],
                PhoneNumber = parts[2],
                OrderIDs = parts.Length > 3 ? parts[3].Split(',').ToList() : new List<string>()
            };
        }
    }
}
