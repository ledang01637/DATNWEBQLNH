using DATN.Shared;
using System.Collections.Generic;
using System;
using DATN.Server.Service;
using System.Linq;
using DATN.Server.Data;
using Microsoft.EntityFrameworkCore;

namespace DATN.Server.Service
{
    public class RewardPointeService
    {
        private AppDBContext _context;
        public RewardPointeService(AppDBContext context)
        {
            _context = context;
        }
        public List<RewardPointe> GetRewardPointe()
        {
            return _context.RewardPointes.ToList();
        }

        public List<RewardPointe> GetRewardPointeLstByCustomer(int cusstomerId)
        {

            return _context.RewardPointes
                .Where(o => o.CustomerId == cusstomerId)
                .ToList() ?? new List<RewardPointe>();
        }
        public RewardPointe AddRewardPointe(RewardPointe RewardPointe)
        {
            _context.Add(RewardPointe);
            _context.SaveChanges();
            return RewardPointe;
        }
        public RewardPointe DeleteRewardPointe(int id)
        {
            var existing = _context.RewardPointes.Find(id);
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
        public RewardPointe GetIdRewardPointe(int id)
        {
            var rewardPointe = _context.RewardPointes.Find(id);
            if (rewardPointe == null)
            {
                return null;
            }
            return rewardPointe;
        }
        public RewardPointe UpdateRewardPointe(int id, RewardPointe update)
        {
            var existing = _context.RewardPointes.Find(id);
            if (existing == null)
            {
                return null;
            }
            existing.CustomerId = update.CustomerId;
            existing.RewardPoint = update.RewardPoint;
            existing.UpdateDate = update.UpdateDate;
            existing.IsDeleted = update.IsDeleted;

            _context.Update(existing);
            _context.SaveChanges();
            return existing;
        }
    }
}
