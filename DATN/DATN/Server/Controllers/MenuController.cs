using Microsoft.AspNetCore.Mvc;
using DATN.Shared;
using Microsoft.AspNetCore.Http;
using System.Collections.Generic;
using DATN.Server.Service;

namespace DATN.Server.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class MenuController : ControllerBase
    {
        private readonly MenuService _MenuService;

        public MenuController(MenuService _menu)
        {
            _MenuService = _menu;
        }

        [HttpGet("GetMenu")]
        public List<Menu> GetMenus()
        {
            return _MenuService.GetMenu();
        }
        [HttpPost("AddMenu")]
        public Menu AddMenu(Menu Menu)
        {
            return _MenuService.AddMenu(new Menu
            {
                MenuName = Menu.MenuName,
                MenuDescription = Menu.MenuDescription,
                IsDelete = Menu.IsDelete,
                PriceCombo = Menu.PriceCombo,

            });
        }

        [HttpGet("{id}")]
        public ActionResult<Menu> GetIdMenu(int id)
        {
            if (id == 0)
            {
                return BadRequest("Value must be...");

            }
            return Ok(_MenuService.GetIdMenu(id));
        }


        [HttpDelete("{id}")]
        public IActionResult DeleteMenu(int id)
        {
            var deleted = _MenuService.DeleteMenu(id);
            if (deleted == null)
            {
                return NotFound("Menu not found");
            }

            return Ok(deleted);
        }

        [HttpPut("{id}")]
        public IActionResult UpdateMenu(int id, [FromBody] Menu updatedMenu)
        {
            var updated = _MenuService.UpdateMenu(id, updatedMenu);
            if (updated == null)
            {
                return NotFound("Menu not found");
            }

            return Ok(updated);
        }
    }
}
