using DATN.Server.Data;
using DATN.Shared;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.Facebook;
using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using System.Security.Claims;
using System.Threading.Tasks;
using System;
using System.Linq;
using System.Text;
using System.Security.Cryptography;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Principal;
using System.Net.Http;
using Microsoft.EntityFrameworkCore;

namespace DATN.Server.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class LoginGGController : ControllerBase
    {
        private readonly IConfiguration _configuration;
        private readonly AppDBContext _appDBContext;

        public LoginGGController(IConfiguration configuration, AppDBContext appDBContext)
        {
            _configuration = configuration;
            _appDBContext = appDBContext;

        }


        [HttpGet("signin-google")]
        public IActionResult SignInWithGoogle()
        {
            var properties = new AuthenticationProperties
            {
                RedirectUri = Url.Action("GoogleResponse")
            };

            return Challenge(properties, GoogleDefaults.AuthenticationScheme);
        }

        [HttpGet("google-response")]
        public async Task<IActionResult> GoogleResponse()
        {
            var result = await HttpContext.AuthenticateAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            if (!result.Succeeded)
            {
                return Ok(new LoginRespone
                {
                    SuccsessFull = false,
                    Error = "Đăng nhập thất bại"
                });
            }

            var emailClaim = result.Principal.FindFirst(ClaimTypes.Email);

            var account = await _appDBContext.Accounts
                .FirstOrDefaultAsync(x => x.Email == emailClaim.Value);

            if (account == null)
            {
                account = new Account
                {
                    AccountType = "customer",
                    CreateDate = DateTime.Now,
                    UpdateDate = DateTime.Now,
                    Email = emailClaim.Value,
                    Password = HashSHA1("123@#$"),
                    IsActive = true,
                    IsDeleted = false
                };
                await _appDBContext.Accounts.AddAsync(account);
                await _appDBContext.SaveChangesAsync();
            }

            var role = await _appDBContext.Roles
                .FirstOrDefaultAsync(r => r.RoleName == "customer");

            if (role == null)
            {
                return BadRequest("Role not found.");
            }

            var roleAccount = new RoleAccount
            {
                RoleId = role.RoleId,
                AccountId = account.AccountId,
                IsDeleted = false
            };

            var existingRoleAccount = await _appDBContext.RoleAccounts
                .FirstOrDefaultAsync(ra => ra.AccountId == account.AccountId && ra.RoleId == role.RoleId);

            if (existingRoleAccount == null)
            {
                await _appDBContext.RoleAccounts.AddAsync(roleAccount);
            }
            else
            {
                existingRoleAccount.IsDeleted = false;
                _appDBContext.RoleAccounts.Update(existingRoleAccount);
            }

            await _appDBContext.SaveChangesAsync();

            var token = GenerateJwtToken(account, roleAccount);

            return LocalRedirectPreserveMethod($"/ggfb-response?token={token}");
        }

        [HttpGet("signin-facebook")]
        public IActionResult SignInWithFacebook()
        {
            var properties = new AuthenticationProperties
            {
                RedirectUri = Url.Action("FacebookResponse")
            };
            return Challenge(properties, FacebookDefaults.AuthenticationScheme);
        }

        [HttpGet("facebook-response")]
        public async Task<IActionResult> FacebookResponse()
        {
            var result = await HttpContext.AuthenticateAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            if (!result.Succeeded)
            {
                return Ok(new LoginRespone
                {
                    SuccsessFull = false,
                    Error = "Đăng nhập thất bại"
                });
            }
            var emailClaim = result.Principal.FindFirst(ClaimTypes.Email);

            var account = await _appDBContext.Accounts
                .FirstOrDefaultAsync(x => x.Email == emailClaim.Value);

            if (account == null)
            {
                account = new Account
                {
                    AccountType = "customer",
                    CreateDate = DateTime.Now,
                    UpdateDate = DateTime.Now,
                    Email = emailClaim.Value,
                    Password = HashSHA1("123@#$"),
                    IsActive = true,
                    IsDeleted = false
                };
                await _appDBContext.Accounts.AddAsync(account);
                await _appDBContext.SaveChangesAsync();
            }

            var role = await _appDBContext.Roles
                .FirstOrDefaultAsync(r => r.RoleName == "customer");

            if (role == null)
            {
                return BadRequest("Role not found.");
            }

            var roleAccount = new RoleAccount
            {
                RoleId = role.RoleId,
                AccountId = account.AccountId,
                IsDeleted = false
            };

            var existingRoleAccount = await _appDBContext.RoleAccounts
                .FirstOrDefaultAsync(ra => ra.AccountId == account.AccountId && ra.RoleId == role.RoleId);

            if (existingRoleAccount == null)
            {
                await _appDBContext.RoleAccounts.AddAsync(roleAccount);
            }
            else
            {
                existingRoleAccount.IsDeleted = false;
                _appDBContext.RoleAccounts.Update(existingRoleAccount);
            }

            await _appDBContext.SaveChangesAsync();

            var token = GenerateJwtToken(account, roleAccount);

            return LocalRedirectPreserveMethod($"/ggfb-response?token={token}");
        }

        private string GenerateJwtToken(Account account, RoleAccount roleAccount)
        {
            if (account == null || roleAccount == null)
            {
                return null;
            }
            var jwt = _configuration.GetSection("Jwt").Get<JWT>();

            var claims = new[]
            {
                new Claim(JwtRegisteredClaimNames.Sub, jwt.Subject),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new Claim(JwtRegisteredClaimNames.Iat, DateTime.UtcNow.ToString()),

                new Claim("Username", account.Email),
                new Claim("AccountId", account.AccountId.ToString()),
                new Claim("AccountType", account.AccountType),
                new Claim("CreateDate", account.CreateDate.ToString()),
                new Claim("UpdateDate", account.UpdateDate.ToString()),
                new Claim("IsActive", account.IsActive.ToString()),

                new Claim("RoleId", roleAccount.RoleId.ToString()),
                new Claim(ClaimTypes.Role, account.AccountType)

            };
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwt.Key));
            var signIn = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                jwt.Issuer,
                jwt.Audience,
                claims,
                expires: DateTime.UtcNow.AddMinutes(60),
                signingCredentials: signIn
            );
            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        private static string HashSHA1(string password)
        {
            using (SHA1 sha1 = SHA1.Create())
            {
                byte[] passwordBytes = Encoding.UTF8.GetBytes(password);
                byte[] hashedBytes = sha1.ComputeHash(passwordBytes);
                password = Convert.ToBase64String(hashedBytes);
            }
            return password;
        }
    }
}
