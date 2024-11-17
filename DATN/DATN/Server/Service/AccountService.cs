using DATN.Shared;
using System.Collections.Generic;
using System;
using DATN.Server.Service;
using System.Linq;
using DATN.Server.Data;

namespace DATN.Server.Service
{
    public class AccountService
    {
        private AppDBContext _context;
        public AccountService(AppDBContext context)
        {
            _context = context;
        }
        public List<Account> GetAccount()
        {
            return _context.Accounts.ToList();
        }

        public Account GetAccountExist(string Email)
        {
            var account = _context.Accounts.FirstOrDefault(x => x.Email == Email);

            return account ?? new Account();
        }


        public Account AddAccount(Account Account)
        {
            _context.Add(Account);
            _context.SaveChanges();
            return Account;
        }
        public Account DeleteAccount(int id)
        {
            var existing = _context.Accounts.Find(id);
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
        public Account GetIdAccount(int id)
        {
            var account = _context.Accounts.Find(id);
            if (account == null)
            {
                return null;
            }
            return account;
        }
        public Account UpdateAccount(int id, Account update)
        {
            var existing = _context.Accounts.Find(id);
            if (existing == null)
            {
                return null;
            }
            existing.AccountType = update.AccountType;
            existing.CreateDate = update.CreateDate;
            existing.UpdateDate = update.UpdateDate;
            existing.IsActive = update.IsActive;
            existing.Email = update.Email;
            existing.Password = update.Password;

            _context.Update(existing);
            _context.SaveChanges();
            return existing;
        }
    }
}
