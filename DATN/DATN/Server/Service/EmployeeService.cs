using DATN.Shared;
using System.Collections.Generic;
using System;
using DATN.Server.Service;
using System.Linq;
using DATN.Server.Data;

namespace DATN.Server.Service
{
    public class EmployeeService
    {
        private AppDBContext _context;
        public EmployeeService(AppDBContext context)
        {
            _context = context;
        }
        public List<Employee> GetEmployee()
        {
            return _context.Employees.ToList();
        }

        public Employee GetEmployeeByAccountId(int accountId)
        {
            var employee = _context.Employees.FirstOrDefault(x => x.AccountId == accountId);

            return employee ?? new Employee();
        }
        public Employee AddEmployee(Employee Employee)
        {
            _context.Add(Employee);
            _context.SaveChanges();
            return Employee;
        }
        public Employee DeleteEmployee(int id)
        {
            var existing = _context.Employees.Find(id);
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
        public Employee GetIdEmployee(int id)
        {
            var employee = _context.Employees.Find(id);
            if (employee == null)
            {
                return null;
            }
            return employee;
        }
        public Employee UpdateEmployee(int id, Employee update)
        {
            var existing = _context.Employees.Find(id);
            if (existing == null)
            {
                return null;
            }
            existing.EmployeeName = update.EmployeeName;
            existing.Address = update.Address;
            existing.PhoneNumber = update.PhoneNumber;
            existing.Email = update.Email;
            existing.Position = update.Position;
            existing.HireDate = update.HireDate;
            existing.Salary = update.Salary;
            existing.IsDeleted = update.IsDeleted;
            existing.AccountId = update.AccountId;

            _context.Update(existing);
            _context.SaveChanges();
            return existing;
        }
    }
}
