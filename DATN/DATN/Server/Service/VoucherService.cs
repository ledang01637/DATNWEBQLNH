using DATN.Shared;
using System.Collections.Generic;
using System;
using DATN.Server.Service;
using System.Linq;
using DATN.Server.Data;

namespace DATN.Server.Service
{
    public class VoucherService
    {
        private AppDBContext _context;
        public VoucherService(AppDBContext context)
        {
            _context = context;
        }
        public List<Voucher> GetVoucher()
        {
            return _context.Vouchers.ToList();
        }
        public Voucher AddVoucher(Voucher Voucher)
        {
            _context.Add(Voucher);
            _context.SaveChanges();
            return Voucher;
        }
        public Voucher DeleteVoucher(int id)
        {
            var existing = _context.Vouchers.Find(id);
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
        public Voucher GetIdVoucher(int id)
        {
            var voucher = _context.Vouchers.Find(id);
            if (voucher == null)
            {
                return null;
            }
            return voucher;
        }
        public Voucher UpdateVoucher(int id, Voucher update)
        {
            var existing = _context.Vouchers.Find(id);
            if (existing == null)
            {
                return null;
            }
            existing.VoucherCode = update.VoucherCode;
            existing.PointRequired = update.PointRequired;
            existing.DiscountValue = update.DiscountValue;
            existing.ExpriationDate = update.ExpriationDate;
            existing.IsAcctive = update.IsAcctive;
            existing.IsDeleted = update.IsDeleted;

            _context.Update(existing);
            _context.SaveChanges();
            return existing;
        }
    }
}
