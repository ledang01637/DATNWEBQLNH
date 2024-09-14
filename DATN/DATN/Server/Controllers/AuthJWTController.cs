using DATN.Shared;

using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;

using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System;
using System.Linq;
using DATN.Server.Data;
using System.Security.Cryptography;

namespace DATN.Server.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthJWTController : ControllerBase
    {
        private readonly IConfiguration _configuration;
        private readonly AppDBContext _appDBContext;

        public AuthJWTController(IConfiguration configuration, AppDBContext appDBContext)
        {
            _configuration = configuration;
            _appDBContext = appDBContext;

        }

        [HttpPost("AuthUser")]
        public IActionResult Login([FromBody] LoginRequest loginModel)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            var isSuccess = ValidateUser(loginModel.Username, loginModel.Password);

            var successLogin = _appDBContext.Accounts
                .FirstOrDefault(x => x.UserName == loginModel.Username && isSuccess);

            if (successLogin != null && successLogin.UserName.Equals("Customer"))
            {
                var token = GenerateJwtTokenV2(successLogin);
                return Ok(new LoginRespone
                {
                    SuccsessFull = true,
                    Token = token
                });
            }

            if(successLogin != null && !successLogin.UserName.Equals("Customer"))
            {
                var roles = _appDBContext.Roles.ToList();
                var roleAccount = _appDBContext.RoleAccounts
                    .AsEnumerable()
                    .FirstOrDefault(a => roles.Any(r => r.RoleId == a.Roleid) && a.AccountId == successLogin.AccountId);


                if (roleAccount != null)
                {
                    var token = GenerateJwtToken(successLogin, roleAccount);
                    return Ok(new LoginRespone
                    {
                        SuccsessFull = true,
                        Token = token
                    });
                }
                else
                {
                    return Ok(new LoginRespone
                    {
                        SuccsessFull = false,
                        Error = "Lỗi quyền truy cập"
                    });
                }
            }else
            {
                return Ok(new LoginRespone
                {
                    SuccsessFull = false,
                    Error = "Tài khoản hoặc mật khẩu không chính xác."
                });
            }
        }

        

        private string GenerateJwtToken(Account account,RoleAccount roleAccount)
        {
            var jwt = _configuration.GetSection("Jwt").Get<JWT>();

            var claims = new[]
            {
                new Claim(JwtRegisteredClaimNames.Sub, jwt.Subject),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new Claim(JwtRegisteredClaimNames.Iat, DateTime.UtcNow.ToString()),
                
                new Claim("Username", account.UserName),
                new Claim("AccountType", account.AccountType),
                new Claim("CreateDate", account.CreateDate.ToString()),
                new Claim("UpdateDate", account.UpdateDate.ToString()),
                new Claim("IsActive", account.IsActive.ToString()),

                new Claim("RoleId", roleAccount.Roleid.ToString()),
                new Claim(ClaimTypes.Role, roleAccount.Roleid.ToString())

            };
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwt.Key));
            var signIn = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                jwt.Issuer,
                jwt.Audience,
                claims,
                expires: DateTime.UtcNow.AddMinutes(30),
                signingCredentials: signIn
            );
            return new JwtSecurityTokenHandler().WriteToken(token);
        }
        private string GenerateJwtTokenV2(Account account)
        {
            var jwt = _configuration.GetSection("Jwt").Get<JWT>();

            var claims = new[]
            {
                new Claim(JwtRegisteredClaimNames.Sub, jwt.Subject),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new Claim(JwtRegisteredClaimNames.Iat, DateTime.UtcNow.ToString()),

                new Claim("Username", account.UserName),
                new Claim(ClaimTypes.Role, "Customer")

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


        public bool ValidateUser(string username, string password)
        {
            using (SHA1 sha1 = SHA1.Create())
            {
                byte[] passwordBytes = Encoding.UTF8.GetBytes(password);
                byte[] hashedBytes = sha1.ComputeHash(passwordBytes);
                password = Convert.ToBase64String(hashedBytes);
            }
            var user = _appDBContext.Accounts.FirstOrDefault(u => u.UserName.Equals(username) && u.Password.Equals(password));
            if (user == null)
                return false;
            else
            {
                return true;
            }  
        }

    }
}
