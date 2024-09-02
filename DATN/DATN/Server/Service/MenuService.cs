using DATN.Shared;
using System.Collections.Generic;
using System;
using DATN.Server.Service;
using System.Linq;
using DATN.Server.Data;

namespace DATN.Server.Service
{
    public class MenuService
    {
        private AppDBContext _context;
        public MenuService(AppDBContext context)
        {
            _context = context;
        }
        public List<Menu> GetMenu()
        {
            return _context.Menus.ToList();
        }
        public Menu AddMenu(Menu Menu)
        {
            _context.Add(Menu);
            _context.SaveChanges();
            return Menu;
        }
        public Menu DeleteMenu(int id)
        {
            var existing = _context.Menus.Find(id);
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
        public Menu GetIdMenu(int id)
        {
            var menu = _context.Menus.Find(id);
            if (menu == null)
            {
                return null;
            }
            return menu;
        }
        public Menu UpdateMenu(int id, Menu update)
        {
            var existing = _context.Menus.Find(id);
            if (existing == null)
            {
                return null;
            }
            existing.MenuName = update.MenuName;
            existing.MenuDescription = update.MenuDescription;
            existing.PriceCombo = update.PriceCombo;
            existing.IsDelete = update.IsDelete;

            _context.Update(existing);
            _context.SaveChanges();
            return existing;
        }
    }
}
