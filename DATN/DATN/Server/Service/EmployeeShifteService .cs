﻿using DATN.Shared;
using System.Collections.Generic;
using System;
using DATN.Server.Service;
using System.Linq;
using DATN.Server.Data;
using Microsoft.EntityFrameworkCore;

namespace DATN.Server.Service
{
    public class EmployeeShifteService
    {
        private AppDBContext _context;
        public EmployeeShifteService(AppDBContext context)
        {
            _context = context;
        }
        public List<EmployeeShifte> GetEmployeeShifte()
        {
            return _context.EmployeeShiftes.ToList();
        }
        public EmployeeShifte AddEmployeeShifte(EmployeeShifte EmployeeShifte)
        {
            EmployeeShifte.IsDeleted = false;
            _context.Add(EmployeeShifte);
            _context.SaveChanges();

            return EmployeeShifte;
        }
        public EmployeeShifte DeleteEmployeeShifte(int id)
        {
            var existing = _context.EmployeeShiftes.Find(id);
            if (existing == null)
            {
                return null;
            }

            existing.IsDeleted = true;
            _context.Update(existing);
            _context.SaveChanges();
            return existing;
        }

        public EmployeeShifte GetIdEmployeeShifte(int id)
        {
            var employeeShifte = _context.EmployeeShiftes.Find(id);
            if (employeeShifte == null)
            {
                return null;
            }
            return employeeShifte;
        }
        public EmployeeShifte UpdateEmployeeShifte(int id, EmployeeShifte update)
        {
            var existing = _context.EmployeeShiftes.Find(id);
            if (existing == null)
            {
                return null;
            }
            existing.ShifteDay = update.ShifteDay;
            existing.EmployeeId = update.EmployeeId;
            existing.ShifteId = update.ShifteId;
            existing.IsDeleted = update.IsDeleted;

            _context.Update(existing);
            _context.SaveChanges();
            return existing;
        }
        public List<EmployeeShifte> GetEmployeeShiftes()
        {
            return _context.EmployeeShiftes.Where(e => !e.IsDeleted).AsNoTracking().ToList();
        }

    }
}
