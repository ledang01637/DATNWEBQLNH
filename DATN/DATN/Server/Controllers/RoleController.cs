using Microsoft.AspNetCore.Mvc;
using DATN.Shared;
using Microsoft.AspNetCore.Http;
using System.Collections.Generic;
using DATN.Server.Service;

namespace DATN.Server.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class RoleController : ControllerBase
    {
        private readonly RoleService _RoleService;

        public RoleController(RoleService _role)
        {
            _RoleService = _role;
        }

        [HttpGet("GetRole")]
        public List<Role> GetRole()
        {
            return _RoleService.GetRole();
        }

        [HttpGet("GetRoleIdCustomer")]
        public ActionResult<int> GetRoleIdCustomer()
        {
            return Ok(_RoleService.GetRoleIdCustomer());
        }

        [HttpPost("AddRole")]
        public Role AddRole(Role Role)
        {
            return _RoleService.AddRole(new Role
            {
                RoleName = Role.RoleName,
                RoleDescription = Role.RoleDescription,
                IsDeleted = Role.IsDeleted,

            });
        }

        [HttpGet("{id}")]
        public ActionResult<Role> GetIdRole(int id)
        {
            if (id == 0)
            {
                return BadRequest("Value must be...");

            }
            return Ok(_RoleService.GetIdRole(id));
        }


        [HttpDelete("{id}")]
        public IActionResult DeleteRole(int id)
        {
            var deleted = _RoleService.DeleteRole(id);
            if (deleted == null)
            {
                return NotFound("Role not found");
            }

            return Ok(deleted);
        }

        [HttpPut("{id}")]
        public IActionResult UpdateRole(int id, [FromBody] Role updatedRole)
        {
            var updated = _RoleService.UpdateRole(id, updatedRole);
            if (updated == null)
            {
                return NotFound("Role not found");
            }

            return Ok(updated);
        }
    }
}
