using DATN.Shared;
using System.Collections.Generic;
using System;
using DATN.Server.Service;
using System.Linq;
using DATN.Server.Data;

namespace DATN.Server.Service
{
    public class ShifteService
    {
        private AppDBContext _context;
        public ShifteService(AppDBContext context)
        {
            _context = context;
        }
        public List<Shifte> GetShifte()
        {
            return _context.Shiftes.ToList();
        }
        public Shifte AddShifte(Shifte Shifte)
        {
            _context.Add(Shifte);
            _context.SaveChanges();
            return Shifte;
        }
        public Shifte DeleteShifte(int id)
        {
            var existing = _context.Shiftes.Find(id);
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
        public Shifte GetIdShifte(int id)
        {
            var shifte = _context.Shiftes.Find(id);
            if (shifte == null)
            {
                return null;
            }
            return shifte;
        }
        public Shifte UpdateShifte(int id, Shifte update)
        {
            var existing = _context.Shiftes.Find(id);
            if (existing == null)
            {
                return null;
            }
            existing.Shifte_Name = update.Shifte_Name;
            existing.StartTime = update.StartTime;
            existing.EndTime = update.EndTime;
            existing.IsDeleted = update.IsDeleted;

            _context.Update(existing);
            _context.SaveChanges();
            return existing;
        }
    }
}
