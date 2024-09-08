using DATN.Shared;
using System.Collections.Generic;
using System;
using DATN.Server.Service;
using System.Linq;
using DATN.Server.Data;

namespace DATN.Server.Service
{
    public class CategoryService
    {
        private AppDBContext _context;
        public CategoryService(AppDBContext context)
        {
            _context = context;
        }
        public List<Category> GetCategory()
        {
            return _context.Categories.ToList();
        }
        public Category AddCategory(Category Category)
        {
            _context.Add(Category);
            _context.SaveChanges();
            return Category;
        }
        public Category DeleteCategory(int id)
        {
            var existingCate = _context.Categories.Find(id);
            if (existingCate == null)
            {
                return null;
            }
            else
            {
                _context.Remove(existingCate);
                _context.SaveChanges();
                return existingCate;
            }
        }
        public Category GetIdCategory(int id)
        {
            var cate = _context.Categories.Find(id);
            if (cate == null)
            {
                return null;
            }
            return cate;
        }
        public Category UpdateCategory(int id, Category updateCategory)
        {
            var existingCate = _context.Categories.Find(id);
            if (existingCate == null)
            {
                return null;
            }
            existingCate.CategoryName = updateCategory.CategoryName;
            existingCate.CategoryDescription = updateCategory.CategoryDescription;
            existingCate.IsDelete = updateCategory.IsDelete;

            _context.Update(existingCate);
            _context.SaveChanges();
            return existingCate;
        }
    }
}
