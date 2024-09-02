using DATN.Shared;
using System.Collections.Generic;
using System;
using DATN.Server.Service;
using System.Linq;
using DATN.Server.Data;

namespace DATN.Server.Service
{
    public class TableService
    {
        private AppDBContext _context;
        public TableService(AppDBContext context)
        {
            _context = context;
        }
        public List<Table> GetTable()
        {
            return _context.Tables.ToList();
        }
        public Table AddTable(Table Table)
        {
            _context.Add(Table);
            _context.SaveChanges();
            return Table;
        }
        public Table DeleteTable(int id)
        {
            var existing = _context.Tables.Find(id);
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
        public Table GetIdTable(int id)
        {
            var table = _context.Tables.Find(id);
            if (table == null)
            {
                return null;
            }
            return table;
        }
        public Table UpdateTable(int id, Table update)
        {
            var existing = _context.Tables.Find(id);
            if (existing == null)
            {
                return null;
            }
            existing.FloorId = update.FloorId;
            existing.TableNumber = update.TableNumber;
            existing.Position = update.Position;
            existing.SeatingCapacity = update.SeatingCapacity;
            existing.Status = update.Status;
            existing.IsDeleted = update.IsDeleted;

            _context.Update(existing);
            _context.SaveChanges();
            return existing;
        }
    }
}
