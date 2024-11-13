using Microsoft.AspNetCore.Mvc;
using DATN.Shared;
using Microsoft.AspNetCore.Http;
using System.Collections.Generic;
using DATN.Server.Service;

namespace DATN.Server.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class MenuItemController : ControllerBase
    {
        private readonly MenuItemService _MenuItemService;

        public MenuItemController(MenuItemService _menuItem)
        {
            _MenuItemService = _menuItem;
        }

        [HttpGet("GetMenuItem")]
        public List<MenuItem> GetMenuItems()
        {
            return _MenuItemService.GetMenuItem();
        }
        [HttpPost("AddMenuItem")]
        public MenuItem AddMenuItem(MenuItem MenuItem)
        {
            return _MenuItemService.AddMenuItem(new MenuItem
            {
                MenuId = MenuItem.MenuId,
                ProductId = MenuItem.ProductId,
                IsDeleted = MenuItem.IsDeleted,
            });
        }

        [HttpGet("{id}")]
        public ActionResult<MenuItem> GetIdMenuItem(int id)
        {
            if (id == 0)
            {
                return BadRequest("Value must be...");

            }
            return Ok(_MenuItemService.GetIdMenuItem(id));
        }


        [HttpDelete("{id}")]
        public IActionResult DeleteMenuItem(int id)
        {
            var deleted = _MenuItemService.DeleteMenuItem(id);
            if (deleted == null)
            {
                return NotFound("MenuItem not found");
            }

            return Ok(deleted);
        }

        [HttpPut("{id}")]
        public IActionResult UpdateMenuItem(int id, [FromBody] MenuItem updatedMenuItem)
        {
            var updated = _MenuItemService.UpdateMenuItem(id, updatedMenuItem);
            if (updated == null)
            {
                return NotFound("MenuItem not found");
            }

            return Ok(updated);
        }
    }
}
