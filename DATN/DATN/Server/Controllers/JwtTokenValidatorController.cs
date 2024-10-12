using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System;
using DATN.Shared;
using Microsoft.Extensions.Configuration;
using DATN.Server.Data;
using Microsoft.AspNetCore.Routing;

namespace DATN.Server.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class JwtTokenValidatorController : ControllerBase
    {
        private readonly IConfiguration _configuration;

        public JwtTokenValidatorController(IConfiguration configuration)
        {
            _configuration = configuration;

        }
        [HttpPost]
        [Route("ValidateToken")]
        public IActionResult ValidateTokenStringee([FromBody] string token)
        {
            if (string.IsNullOrEmpty(token))
            {
                return BadRequest("Token is null or empty.");
            }
            if (ValidateToken(token) == null)
            {
                return BadRequest("ValidateToken is null or empty.");
            }
            return Ok(token);
        }


        public ClaimsPrincipal ValidateToken(string token)
        {

            var jwt = _configuration.GetSection("Jwt").Get<JWT>();
            string secretKey = jwt.Key;
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.UTF8.GetBytes(secretKey);

            var validationParameters = new TokenValidationParameters
            {
                ValidateIssuer = false,
                ValidateAudience = false,
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(key),
                ValidateLifetime = true,
                ClockSkew = TimeSpan.Zero
            };

            try
            {
                var principal = tokenHandler.ValidateToken(token, validationParameters, out SecurityToken validatedToken);

                if (validatedToken is JwtSecurityToken jwtToken)
                {
                    return principal;
                }
            }
            catch (SecurityTokenExpiredException)
            {
                Console.WriteLine("Token has expired.");
            }
            catch (SecurityTokenException)
            {
                Console.WriteLine("Token is invalid.");
            }

            return null;
        }
    }
}
