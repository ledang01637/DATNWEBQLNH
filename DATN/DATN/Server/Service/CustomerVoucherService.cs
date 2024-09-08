using DATN.Shared;
using System.Collections.Generic;
using System;
using DATN.Server.Service;
using System.Linq;
using DATN.Server.Data;

namespace DATN.Server.Service
{
    public class CustomerVoucherService
    {
        private AppDBContext _context;
        public CustomerVoucherService(AppDBContext context)
        {
            _context = context;
        }
        public List<CustomerVoucher> GetCustomerVoucher()
        {
            return _context.CustomerVouchers.ToList();
        }
        public CustomerVoucher AddCustomerVoucher(CustomerVoucher CustomerVoucher)
        {
            _context.Add(CustomerVoucher);
            _context.SaveChanges();
            return CustomerVoucher;
        }
        public CustomerVoucher DeleteCustomerVoucher(int id)
        {
            var existing = _context.CustomerVouchers.Find(id);
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
        public CustomerVoucher GetIdCustomerVoucher(int id)
        {
            var customerVoucher = _context.CustomerVouchers.Find(id);
            if (customerVoucher == null)
            {
                return null;
            }
            return customerVoucher;
        }
        public CustomerVoucher UpdateCustomerVoucher(int id, CustomerVoucher update)
        {
            var existing = _context.CustomerVouchers.Find(id);
            if (existing == null)
            {
                return null;
            }
            existing.CustomerId = update.CustomerId;
            existing.VoucherId = update.VoucherId;
            existing.Status = update.Status;
            existing.RedeemDate = update.RedeemDate;
            existing.ExpirationDate = update.ExpirationDate;

            _context.Update(existing);
            _context.SaveChanges();
            return existing;
        }
    }
}
