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
using System.Security.Policy;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;

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
            var isSuccess = ValidateUser(loginModel.Email, loginModel.Password);

            var successLogin = _appDBContext.Accounts
                .FirstOrDefault(x => x.Email == loginModel.Email && isSuccess);

            if (successLogin != null && successLogin.AccountType.Trim().ToLower().Equals("noaccount"))
            {
                var token = GenerateJwtTokenV2(successLogin);
                return Ok(new LoginRespone
                {
                    SuccsessFull = true,
                    Token = token
                });
            }
            else if(successLogin != null)
            {
                var roles = _appDBContext.Roles.ToList();
                var roleAccount = _appDBContext.RoleAccounts
                    .AsEnumerable()
                    .FirstOrDefault(a => roles.Any(r => r.RoleId == a.RoleId) && a.AccountId == successLogin.AccountId);


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
            }
            else
            {
                return Ok(new LoginRespone
                {
                    SuccsessFull = false,
                    Error = "Tài khoản hoặc mật khẩu không chính xác."
                });
            }


        }
        private string GenerateJwtToken(Account account, RoleAccount roleAccount)
        {
            if(account == null || roleAccount == null)
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
        private string GenerateJwtTokenV2(Account account)
        {
            var jwt = _configuration.GetSection("Jwt").Get<JWT>();

            var claims = new[]
            {
                new Claim(JwtRegisteredClaimNames.Sub, jwt.Subject),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new Claim(JwtRegisteredClaimNames.Iat, DateTime.UtcNow.ToString()),

                new Claim("AccountType", account.AccountType),
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

        //JWT Stringee
        [HttpPost]
        [Route("GenerateQrToken")]
        public IActionResult GenerateQrToken(QR qr)
        {
            var token = AccessTokenStringee(qr.NumberTable.ToString());
            if (token != null)
            {
                return Ok(new QRResponse
                {
                    IsSuccessFull = true,
                    Token = token
                });
            }
            else
            {
                return BadRequest(new QRResponse
                {
                    IsSuccessFull = false
                });
            }

        }

        [HttpPost]
        [Route("ManagerToken")]
        public IActionResult ManagerToken([FromBody] LoginRequest loginModel)
        {


            var isSuccess = ValidateUser(loginModel.Email, loginModel.Password);

            var successLogin = _appDBContext.Accounts
                .FirstOrDefault(x => x.Email == loginModel.Email && isSuccess);

            if (successLogin != null)
            {
                var token = AccessTokenStringee(successLogin.Email);

                return Ok(new QRResponse
                {
                    IsSuccessFull = true,
                    Token = token
                });
            }
            return BadRequest("Account Not Valid");

        }

        private string AccessTokenStringee(string userId)
        {

            string _apiKeySid = "SK.0.JswyWnoTcY3sIq71enUhZfI0Iwr9cGU";
            string _apiKeySecret= "NE1lUFAxemNGd0pCQ3RYWEp5VEJQZlV5bmlCV3pkaw==";
;
            var claims = new[]
            {
                new Claim(JwtRegisteredClaimNames.Jti, $"{_apiKeySid}-{DateTimeOffset.UtcNow.ToUnixTimeSeconds()}"),
                new Claim(JwtRegisteredClaimNames.Iss, _apiKeySid),
                new Claim("userId", userId)
            };


            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_apiKeySecret));
            var signIn = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                _apiKeySid,
                _apiKeySid,
                claims,
                expires: DateTimeOffset.UtcNow.AddMinutes(60).UtcDateTime,
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
            var user = _appDBContext.Accounts.FirstOrDefault(u => u.Email.Equals(username) && u.Password.Trim().Equals(password.Trim()));
            if (user == null)
                return false;
            else
            {
                return true;
            }
        }

    }
}
