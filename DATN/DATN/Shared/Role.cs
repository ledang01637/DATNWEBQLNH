
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace DATN.Shared
{
    public class Role
    {
        [Key]
        public int RoleId { get; set; }
        public string RoleName { get; set; }
        public string RoleDescription { get; set; }
        public bool IsDeleted { get; set; }

        public virtual ICollection<RoleAccount> RoleAccounts { get; set; }
    }
}


