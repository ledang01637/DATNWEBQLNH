using Microsoft.AspNetCore.Mvc;
using DATN.Shared;
using Microsoft.AspNetCore.Http;
using System.Collections.Generic;
using DATN.Server.Service;

namespace DATN.Server.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AccountController : ControllerBase
    {
        private readonly AccountService _AccountService;

        public AccountController(AccountService _acount)
        {
            _AccountService = _acount;
        }

        [HttpGet("GetAccount")]
        public List<Account> GetAccounts()
        {
            return _AccountService.GetAccount();
        }
        [HttpPost("AddAccount")]
        public Account AddAccount(Account Account)
        {
            return _AccountService.AddAccount(new Account
            {
                AccountType = Account.AccountType,
                CreateDate = Account.CreateDate,
                UpdateDate = Account.UpdateDate,
                IsActive = Account.IsActive,
                UserName = Account.UserName,
                Password = Account.Password,

            });
        }

        [HttpGet("{id}")]
        public ActionResult<Account> GetIdAccount(int id)
        {
            if (id == 0)
            {
                return BadRequest("Value must be...");

            }
            return Ok(_AccountService.GetIdAccount(id));
        }


        [HttpDelete("{id}")]
        public IActionResult DeleteAccount(int id)
        {
            var deletedCategory = _AccountService.DeleteAccount(id);
            if (deletedCategory == null)
            {
                return NotFound("Account not found");
            }

            return Ok(deletedCategory);
        }

        [HttpPut("{id}")]
        public IActionResult UpdateAccount(int id, [FromBody] Account updated)
        {
            var newupdate = _AccountService.UpdateAccount(id, updated);
            if (newupdate == null)
            {
                return NotFound("Account not found");
            }

            return Ok(newupdate);
        }
    }
}
