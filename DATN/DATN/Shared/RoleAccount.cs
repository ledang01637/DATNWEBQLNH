
using System.ComponentModel.DataAnnotations;

namespace DATN.Shared
{
    public class RoleAccount
    {
        [Key]
        public int RoleAccountId { get; set; }
        public int RoleId { get; set; }
        public int AccountId { get; set; }
        public bool IsDeleted { get; set; }

        public virtual Role Roles { get; set; }
        public virtual Account Accounts { get; set; }
    }
}
