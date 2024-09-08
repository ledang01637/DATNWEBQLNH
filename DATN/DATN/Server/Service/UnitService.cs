using DATN.Shared;
using System.Collections.Generic;
using System;
using DATN.Server.Service;
using System.Linq;
using DATN.Server.Data;

namespace DATN.Server.Service
{
    public class UnitService
    {
        private AppDBContext _context;
        public UnitService(AppDBContext context)
        {
            _context = context;
        }
        public List<Unit> GetUnit()
        {
            return _context.Units.ToList();
        }
        public Unit AddUnit(Unit Unit)
        {
            _context.Add(Unit);
            _context.SaveChanges();
            return Unit;
        }
        public Unit DeleteUnit(int id)
        {
            var existing = _context.Units.Find(id);
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
        public Unit GetIdUnit(int id)
        {
            var unit = _context.Units.Find(id);
            if (unit == null)
            {
                return null;
            }
            return unit;
        }
        public Unit UpdateUnit(int id, Unit update)
        {
            var existing = _context.Units.Find(id);
            if (existing == null)
            {
                return null;
            }
            existing.UnitName = update.UnitName;
            existing.UnitDescription = update.UnitDescription;

            _context.Update(existing);
            _context.SaveChanges();
            return existing;
        }
    }
}
