using DATN.Shared;

using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;

using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System;
using System.Linq;

namespace DATN.Server.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthJWTController : ControllerBase
    {
        private readonly IConfiguration _configuration;
        //private readonly AppDBContext _appDBContext;    
        private User userLogin = new User();

        public AuthJWTController(IConfiguration configuration/* AppDBContext appDBContext,*/)
        {
            _configuration = configuration;
            //_appDBContext = appDBContext;
            
        }

        [HttpPost("AuthUser")]
        public IActionResult Login([FromBody] LoginRequest loginModel)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            var isSuccess = ValidateUser(loginModel.Email, loginModel.Password);

            //var successLogin = _appDBContext.User
            //    .FirstOrDefault(x => x.Email == loginModel.Email && isSuccess);
            var successLogin = liststaticuser.Users
                .FirstOrDefault(x => x.Email == loginModel.Email && isSuccess);

            if (successLogin != null)
            {
                var token = GenerateJwtToken(successLogin);
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
                    Error = "Tài khoản hoặc mật khẩu không chính xác."
                });
            }
        }

        

        private string GenerateJwtToken(User user)
        {
            var jwt = _configuration.GetSection("Jwt").Get<Jwt>();

            var claims = new[]
            {
                new Claim(JwtRegisteredClaimNames.Sub, jwt.Subject),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new Claim(JwtRegisteredClaimNames.Iat, DateTime.UtcNow.ToString()),
                
                new Claim("Email", user.Email),
                
                new Claim("RoleId", user.RoleId.ToString()),
                new Claim(ClaimTypes.Role, user.RoleId.ToString())
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
        public bool ValidateUser(string username, string password)
        {
            //var user = _appDBContext.User.FirstOrDefault(u => u.Email == username);
            var user = liststaticuser.Users.FirstOrDefault(x => x.Email == username && x.Password == password); 
            if (user == null)
                return false;
            else
            {
                return true;
            }

            
        }

    }
}
