using DATN.Shared;
using System.Collections.Generic;
using System;
using DATN.Server.Service;
using System.Linq;
using DATN.Server.Data;

namespace DATN.Server.Service
{
    public class RoleAccountService
    {
        private AppDBContext _context;
        public RoleAccountService(AppDBContext context)
        {
            _context = context;
        }
        public List<RoleAccount> GetRoleAccount()
        {
            return _context.RoleAccounts.ToList();
        }
        public RoleAccount AddRoleAccount(RoleAccount RoleAccount)
        {
            _context.Add(RoleAccount);
            _context.SaveChanges();
            return RoleAccount;
        }
        public RoleAccount DeleteRoleAccount(int id)
        {
            var existing = _context.RoleAccounts.Find(id);
            if (existing == null)
            {
                return null;
            }
            else
            {
                _context.Remove(existing);
                _context.SaveChanges();
                return existing;
            }
        }
        public RoleAccount GetIdRoleAccount(int id)
        {
            var role = _context.RoleAccounts.Find(id);
            if (role == null)
            {
                return null;
            }
            return role;
        }
        public RoleAccount UpdateRoleAccount(int id, RoleAccount update)
        {
            var existing = _context.RoleAccounts.Find(id);
            if (existing == null)
            {
                return null;
            }
            existing.Roleid = update.Roleid;
            existing.AccountId = update.AccountId;
            existing.IsActive = update.IsActive;

            _context.Update(existing);
            _context.SaveChanges();
            return existing;
        }
    }
}
