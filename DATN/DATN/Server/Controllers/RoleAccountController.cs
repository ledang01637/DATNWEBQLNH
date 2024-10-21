using Microsoft.AspNetCore.Mvc;
using DATN.Shared;
using Microsoft.AspNetCore.Http;
using System.Collections.Generic;
using DATN.Server.Service;

namespace DATN.Server.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class RoleAccountController : ControllerBase
    {
        private readonly RoleAccountService _RoleAccountService;

        public RoleAccountController(RoleAccountService _roleAccount)
        {
            _RoleAccountService = _roleAccount;
        }

        [HttpGet("GetRoleAccount")]
        public List<RoleAccount> GetRoleAccount()
        {
            return _RoleAccountService.GetRoleAccount();
        }

        [HttpPost("AddRoleAccount")]
        public RoleAccount AddRoleAccount(RoleAccount RoleAccount)
        {
            return _RoleAccountService.AddRoleAccount(new RoleAccount
            {
                RoleId = RoleAccount.RoleId,
                AccountId = RoleAccount.AccountId,
                IsDeleted = RoleAccount.IsDeleted
            });
        }

        [HttpGet("{id}")]
        public ActionResult<RoleAccount> GetIdRoleAccount(int id)
        {
            if (id == 0)
            {
                return BadRequest("Value must be...");

            }
            return Ok(_RoleAccountService.GetIdRoleAccount(id));
        }


        [HttpDelete("{id}")]
        public IActionResult DeleteRoleAccount(int id)
        {
            var deleted = _RoleAccountService.DeleteRoleAccount(id);
            if (deleted == null)
            {
                return NotFound("RoleAccount not found");
            }

            return Ok(deleted);
        }

        [HttpPut("{id}")]
        public IActionResult UpdateRoleAccount(int id, [FromBody] RoleAccount updatedRoleAccount)
        {
            var updated = _RoleAccountService.UpdateRoleAccount(id, updatedRoleAccount);
            if (updated == null)
            {
                return NotFound("RoleAccount not found");
            }

            return Ok(updated);
        }
    }
}
