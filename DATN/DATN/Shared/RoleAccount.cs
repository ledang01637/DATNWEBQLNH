using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DATN.Shared
{
    public class RoleAccount
    {
        [Key]
        public int RoleaccountId { get; set; }
        public int Roleid { get; set; }
        public int AccountId { get; set; }
        public bool IsActive { get; set; }

        public Role Roles { get; set; }
        public Account Accounts { get; set; }
    }
}
