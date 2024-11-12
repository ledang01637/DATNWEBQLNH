using DATN.Shared;
using System.Collections.Generic;
using System;
using DATN.Server.Service;
using System.Linq;
using DATN.Server.Data;
using Microsoft.EntityFrameworkCore;

namespace DATN.Server.Service
{
    public class OrderItemService
    {
        private AppDBContext _context;
        public OrderItemService(AppDBContext context)
        {
            _context = context;
        }
        public List<OrderItem> GetOrderItem()
        {
            return _context.OrderItems.ToList();
        }

        public List<OrderItem> GetOrderItemInclude(int orderId)
        {
            var orderItems = _context.OrderItems
                .Where(oi => oi.OrderId == orderId)
                .Include(oi => oi.Products)
                    .ThenInclude(p => p.Units)
                .ToList();

            return orderItems ?? new List<OrderItem>();
        }

        public OrderItem GetByOrderIdAndProductId(int orderId, int productId)
        {
            var orderItem = _context.OrderItems
                           .FirstOrDefault(o => o.OrderId == orderId && o.ProductId == productId);
            return orderItem ?? new OrderItem();
        }


        public OrderItem AddOrderItem(OrderItem OrderItem)
        {
            _context.Add(OrderItem);
            _context.SaveChanges();
            return OrderItem;
        }
        public OrderItem DeleteOrderItem(int id)
        {
            var existing = _context.OrderItems.Find(id);
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
        public OrderItem GetIdOrderItem(int id)
        {
            var orderItem = _context.OrderItems.Find(id);
            if (orderItem == null)
            {
                return null;
            }
            return orderItem;
        }
        public OrderItem UpdateOrderItem(int id, OrderItem update)
        {
            var existing = _context.OrderItems.Find(id);
            if (existing == null)
            {
                return null;
            }
            existing.OrderId = update.OrderId;
            existing.ProductId = update.ProductId;
            existing.Quantity = update.Quantity;
            existing.Price = update.Price;
            existing.TotalPrice = update.TotalPrice;

            _context.Update(existing);
            _context.SaveChanges();
            return existing;
        }
    }
}
