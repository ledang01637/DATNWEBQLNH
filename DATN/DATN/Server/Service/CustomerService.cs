using DATN.Shared;
using System.Collections.Generic;
using System;
using DATN.Server.Service;
using System.Linq;
using DATN.Server.Data;

namespace DATN.Server.Service
{
    public class CustomerService
    {
        private AppDBContext _context;
        public CustomerService(AppDBContext context)
        {
            _context = context;
        }
        public List<Customer> GetCustomer()
        {
            return _context.Customers.ToList();
        }
        public Customer AddCustomer(Customer Customer)
        {
            _context.Add(Customer);
            _context.SaveChanges();
            return Customer;
        }
        public Customer DeleteCustomer(int id)
        {
            var existing = _context.Customers.Find(id);
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
        public Customer GetIdCustomer(int id)
        {
            var customer = _context.Customers.Find(id);
            if (customer == null)
            {
                return null;
            }
            return customer;
        }
        public Customer UpdateCustomer(int id, Customer update)
        {
            var existing = _context.Customers.Find(id);
            if (existing == null)
            {
                return null;
            }
            existing.CustomerName = update.CustomerName;
            existing.PhoneNumber = update.PhoneNumber;
            existing.Address = update.Address;
            existing.Email = update.Email;
            existing.IsDeleted = update.IsDeleted;
            existing.AccountId = update.AccountId;

            _context.Update(existing);
            _context.SaveChanges();
            return existing;
        }
    }
}
