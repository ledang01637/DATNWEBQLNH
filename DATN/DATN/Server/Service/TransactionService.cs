using DATN.Server.Data;
using DATN.Shared;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;

namespace DATN.Server.Service
{
    public class TransactionService
    {
        private AppDBContext _context;
        public TransactionService(AppDBContext context)
        {
            _context = context;
        }
        public List<Transaction> GetTransaction()
        {
            return _context.Transactions.ToList();
        }

        public Transaction AddTransaction(Transaction Transaction)
        {
            _context.Add(Transaction);
            _context.SaveChanges();
            return Transaction;
        }
        public Transaction DeleteTransaction(int id)
        {
            var existing = _context.Transactions.Find(id);
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
        public Transaction GetIdTransaction(int id)
        {
            var Transaction = _context.Transactions.Find(id);
            if (Transaction == null)
            {
                return null;
            }
            return Transaction;
        }
        public Transaction UpdateTransaction(int id, Transaction update)
        {
            var existing = _context.Transactions.Find(id);
            if (existing == null)
            {
                return null;
            }
            existing.ReservationId = update.ReservationId;
            existing.Amount = update.Amount;
            existing.PaymentStatus = update.PaymentStatus;
            existing.PaymentDate = update.PaymentDate;

            _context.Update(existing);
            _context.SaveChanges();
            return existing;
        }
    }
}
