﻿using DATN.Shared;
using System.Collections.Generic;
using System;
using DATN.Server.Service;
using System.Linq;
using DATN.Server.Data;
using Microsoft.EntityFrameworkCore;

namespace DATN.Server.Service
{
    public class OrderService
    {
        private AppDBContext _context;
        public OrderService(AppDBContext context)
        {
            _context = context;
        }
        public List<Order> GetOrder()
        { 
            return _context.Orders.ToList(); 
        }
        public Order AddOrder(Order Order)
        {
            _context.Add(Order);
            _context.SaveChanges();
            return Order;
        }
        public Order DeleteOrder(int id)
        {
            var existing = _context.Orders.Find(id);
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
        public Order GetOrderStatus(int tableId, string status)
        {
            var order = _context.Orders.Where(o => o.TableId == tableId && o.Status.Equals(status)).FirstOrDefault();

            return order ?? new Order();
        }

        public Order GetOrderInvoice(int orderId)
        {
            var order = _context.Orders
                .Include(a => a.OrderItems)
                .ThenInclude(b => b.Products)
                .FirstOrDefault(o => o.OrderId == orderId);

            return order ?? new Order();
        }

        public Order GetOrderByTable(int orderId)
        {
            var order = _context.Orders.Where(o => o.OrderId == orderId).FirstOrDefault();
            return order ?? new Order();
        }

        public List<Order> GetOrderLstInclude()
        {
            const string ProcessingStatus = "processing";

            return _context.Orders
                .Include(o => o.OrderItems)
                .ThenInclude(oi => oi.Products)
                .Where(o => o.Status == ProcessingStatus)
                .ToList() ?? new List<Order> ();
        }
        public List<Order> GetOrderLstByCustomer(int customerId)
        {
            return _context.Orders
                .Include(o => o.RewardPointes)
                .Include(o => o.OrderItems)
                .ThenInclude(oi => oi.Products)
                .Where(o => o.CustomerId == customerId)
                .OrderByDescending(o => o.CreateDate)
                .ToList() ?? new List<Order>();
        }

        public Order GetIdOrder(int id)
        {
            var order = _context.Orders.Find(id);

            return order ?? new Order();
        }
        public Order UpdateOrder(int id, Order update)
        {
            var existing = _context.Orders.Find(id);
            if (existing == null)
            {
                return null;
            }
            existing.TableId = update.TableId;
            existing.CreateDate = update.CreateDate;
            existing.TotalAmount = update.TotalAmount;
            existing.Status = update.Status;
            existing.CustomerId = update.CustomerId;
            existing.PaymentMethod = update.PaymentMethod;
            existing.CustomerVoucherId = update.CustomerVoucherId;
            existing.EmployeeId = update.EmployeeId;
            existing.IsDeleted = update.IsDeleted;
            existing.Note = update.Note;
            

            _context.Update(existing);
            _context.SaveChanges();
            return existing;
        }
    }
}
