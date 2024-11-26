using DATN.Shared;
using System.Collections.Generic;
using System;
using DATN.Server.Service;
using System.Linq;
using DATN.Server.Data;
using Microsoft.EntityFrameworkCore;

namespace DATN.Server.Service
{
    public class RoleService
    {
        private AppDBContext _context;
        public RoleService(AppDBContext context)
        {
            _context = context;
        }
        public List<Role> GetRole()
        {
            return _context.Roles.ToList();
        }

        public int GetRoleIdCustomer()
        {
            var role = _context.Roles.FirstOrDefault(r => r.RoleName.Equals("customer"));
            return role == null ? 0 : role.RoleId;
        }
        public Role AddRole(Role Role)
        {
            _context.Add(Role);
            _context.SaveChanges();
            return Role;
        }
        public Role DeleteRole(int id)
        {
            var existing = _context.Roles.Find(id);
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
        public Role GetIdRole(int id)
        {
            var role = _context.Roles.Find(id);
            if (role == null)
            {
                return null;
            }
            return role;
        }
        public Role UpdateRole(int id, Role update)
        {
            var existing = _context.Roles.Find(id);
            if (existing == null)
            {
                return null;
            }
            existing.RoleName = update.RoleName;
            existing.RoleDescription = update.RoleDescription;
            existing.IsDeleted = update.IsDeleted;

            _context.Update(existing);
            _context.SaveChanges();
            return existing;
        }
    }
}
