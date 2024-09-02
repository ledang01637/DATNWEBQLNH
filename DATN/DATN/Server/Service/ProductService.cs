using DATN.Shared;
using System.Collections.Generic;
using System;
using DATN.Server.Service;
using System.Linq;
using DATN.Server.Data;

namespace DATN.Server.Service
{
    public class ProductService
    {
        private AppDBContext _context;
        public ProductService(AppDBContext context)
        {
            _context = context;
        }
        public List<Product> GetProduct()
        {
            return _context.Products.ToList();
        }
        public Product AddProduct(Product Product)
        {
            _context.Add(Product);
            _context.SaveChanges();
            return Product;
        }
        public Product DeleteProduct(int id)
        {
            var existing = _context.Products.Find(id);
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
        public Product GetIdProduct(int id)
        {
            var product = _context.Products.Find(id);
            if (product == null)
            {
                return null;
            }
            return product;
        }
        public Product UpdateProduct(int id, Product update)
        {
            var existing = _context.Products.Find(id);
            if (existing == null)
            {
                return null;
            }
            existing.UnitId = update.UnitId;
            existing.ProductName = update.ProductName;
            existing.Price = update.Price;
            existing.CategoryId = update.CategoryId;
            existing.ProductDescripntion = update.ProductDescripntion;
            existing.ProductImage = update.ProductImage;
            existing.IsDelete = update.IsDelete;

            _context.Update(existing);
            _context.SaveChanges();
            return existing;
        }
    }
}
