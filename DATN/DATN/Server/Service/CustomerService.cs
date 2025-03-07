﻿using DATN.Shared;
using System.Collections.Generic;
using System;
using DATN.Server.Service;
using System.Linq;
using DATN.Server.Data;
using Microsoft.EntityFrameworkCore;

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

        public Customer GetCustomerExist(Customer _customer)
        {
            var customer = _context.Customers.FirstOrDefault(a => a.Email.Equals(_customer.Email) || a.PhoneNumber.Equals(_customer.PhoneNumber));
            return customer ?? new Customer();
        }

        public Customer GetCustomerExistByEmail(string _customerEmail)
        {
            var customer = _context.Customers.FirstOrDefault(a => a.Email.Equals(_customerEmail));
            return customer ?? new Customer();
        }

        public Customer GetCustomerByEmail(string email)
        {
            return _context.Customers.FirstOrDefault(c => c.Email == email);
        }
        public Customer GetCustomerByAccountId(int accountId)
        {
            var customer = _context.Customers.Include(a => a.Accounts).FirstOrDefault(a => a.AccountId == accountId);

            return customer ?? new Customer();
        }

        public List<Customer> GetCustomerInclude()
        {
            return _context.Customers.Include(c => c.Accounts).ToList();
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
            existing.TotalRewardPoint = update.TotalRewardPoint;

            _context.Update(existing);
            _context.SaveChanges();
            return existing;
        }
    }
}
