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
        public int RoleId { get; set; }
        public int AccountId { get; set; }
        public bool IsDeleted { get; set; }

        public virtual Role Roles { get; set; }
        public virtual Account Accounts { get; set; }
    }
}
