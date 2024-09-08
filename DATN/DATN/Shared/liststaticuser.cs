using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DATN.Shared
{
    public class liststaticuser
    {
        public static List<User> Users = new List<User>
        {
            new User { Email = "vinh@vinh.com", PhoneNumber= "0789456123", Password = "123", Username = "User1", RoleId = 1, CreatedAt = DateTime.Now, AccountType = "Thông thường" },
            new User { Email = "test2@example.com", PhoneNumber= "0784848858", Password = "password2", Username = "User2", RoleId = 2, CreatedAt = DateTime.Now, AccountType = "Admin" }
        };
    }
}
