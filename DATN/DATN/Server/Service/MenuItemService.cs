using DATN.Shared;
using System.Collections.Generic;
using System;
using DATN.Server.Service;
using System.Linq;
using DATN.Server.Data;

namespace DATN.Server.Service
{
    public class MenuItemService
    {
        private AppDBContext _context;
        public MenuItemService(AppDBContext context)
        {
            _context = context;
        }
        public List<MenuItem> GetMenuItem()
        {
            return _context.MenuItems.ToList();
        }
        public MenuItem AddMenuItem(MenuItem MenuItem)
        {
            _context.Add(MenuItem);
            _context.SaveChanges();
            return MenuItem;
        }
        public MenuItem DeleteMenuItem(int id)
        {
            var existing = _context.MenuItems.Find(id);
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
        public MenuItem GetIdMenuItem(int id)
        {
            var menuItem = _context.MenuItems.Find(id);
            if (menuItem == null)
            {
                return null;
            }
            return menuItem;
        }
        public MenuItem UpdateMenuItem(int id, MenuItem update)
        {
            var existing = _context.MenuItems.Find(id);
            if (existing == null)
            {
                return null;
            }
            existing.MenuId = update.MenuId;
            existing.Products = update.Products;
            existing.IsDelete = update.IsDelete;

            _context.Update(existing);
            _context.SaveChanges();
            return existing;
        }
    }
}
