using Microsoft.AspNetCore.Mvc;
using DATN.Shared;
using Microsoft.AspNetCore.Http;
using System.Collections.Generic;
using DATN.Server.Service;

namespace DATN.Server.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CategoryController : ControllerBase
    {
        private readonly CategoryService _CategoryService;

        public CategoryController(CategoryService _category)
        {
            _CategoryService = _category;
        }

        [HttpGet("GetCategories")]
        public List<Category> GetCategories()
        {
            return _CategoryService.GetCategory();
        }
        [HttpPost("AddCategory")]
        public Category AddCategory(Category Category)
        {
            return _CategoryService.AddCategory(new Category
            {
                CategoryName = Category.CategoryName,
                CategoryDescription = Category.CategoryDescription,
                IsDelete = Category.IsDelete

            });
        }

        [HttpGet("GetCategoryById/{id}")]
        public ActionResult<Category> GetId(int id)
        {
            if (id == 0)
            {
                return BadRequest("Value must be...");

            }
            return Ok(_CategoryService.GetIdCategory(id));
        }


        [HttpDelete("{id}")]
        public IActionResult Delete(int id)
        {
            var deletedCategory = _CategoryService.DeleteCategory(id);
            if (deletedCategory == null)
            {
                return NotFound("Category not found");
            }

            return Ok(deletedCategory);
        }

        [HttpPut("EditCategory/{id}")]
        public IActionResult Update(int id, [FromBody] Category updatedCategory)
        {
            var updatedLoai = _CategoryService.UpdateCategory(id, updatedCategory);
            if (updatedLoai == null)
            {
                return NotFound("Category not found");
            }

            return Ok(updatedLoai);
        }
    }
}
