using Microsoft.AspNetCore.Mvc;
using DATN.Shared;
using Microsoft.AspNetCore.Http;
using System.Collections.Generic;
using DATN.Server.Service;

namespace DATN.Server.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProductController : ControllerBase
    {
        private readonly ProductService _ProductService;

        public ProductController(ProductService _product)
        {
            _ProductService = _product;
        }

        [HttpGet("GetProduct")]
        public List<Product> GetProducts()
        {
            return _ProductService.GetProduct();
        }
        [HttpPost("AddProduct")]
        public Product AddProduct(Product Product)
        {
            return _ProductService.AddProduct(new Product
            {
                UnitId = Product.UnitId,
                ProductName = Product.ProductName,
                Price = Product.Price,
                CategoryId = Product.CategoryId,
                ProductDescription = Product.ProductDescription,
                ProductImage = Product.ProductImage,
                IsDeleted = Product.IsDeleted
            });
        }

        [HttpGet("{id}")]
        public ActionResult<Product> GetIdProduct(int id)
        {
            if (id == 0)
            {
                return BadRequest("Value must be...");

            }
            return Ok(_ProductService.GetIdProduct(id));
        }


        [HttpDelete("{id}")]
        public IActionResult DeleteProduct(int id)
        {
            var deleted = _ProductService.DeleteProduct(id);
            if (deleted == null)
            {
                return NotFound("Product not found");
            }

            return Ok(deleted);
        }

        [HttpPut("{id}")]
        public IActionResult UpdateProduct(int id, [FromBody] Product updatedProduct)
        {
            var updated = _ProductService.UpdateProduct(id, updatedProduct);
            if (updated == null)
            {
                return NotFound("Product not found");
            }

            return Ok(updated);
        }
    }
}
