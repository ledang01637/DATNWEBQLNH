using DATN.Shared;
using System.Collections.Generic;
using System;
using DATN.Server.Service;
using System.Linq;
using DATN.Server.Data;

namespace DATN.Server.Service
{
    public class FloorService
    {
        private AppDBContext _context;
        public FloorService(AppDBContext context)
        {
            _context = context;
        }
        public List<Floor> GetFloor()
        {
            return _context.Floors.ToList();
        }
        public Floor AddFloor(Floor Floor)
        {
            _context.Add(Floor);
            _context.SaveChanges();
            return Floor;
        }
        public Floor DeleteFloor(int id)
        {
            var existing = _context.Floors.Find(id);
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
        public Floor GetIdFloor(int id)
        {
            var floor = _context.Floors.Find(id);
            if (floor == null)
            {
                return null;
            }
            return floor;
        }
        public Floor UpdateFloor(int id, Floor update)
        {
            var existing = _context.Floors.Find(id);
            if (existing == null)
            {
                return null;
            }
            existing.NumberFloor = update.NumberFloor;
            existing.IsDeleted = update.IsDeleted;

            _context.Update(existing);
            _context.SaveChanges();
            return existing;
        }
    }
}
