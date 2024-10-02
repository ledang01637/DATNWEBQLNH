using Microsoft.AspNetCore.Components.Authorization;
using System;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace DATN.Client.Service
{
    public class CustomAuthenticationStateProvider : AuthenticationStateProvider
    {
        private readonly LocalStorageService _localStorageService;

        public CustomAuthenticationStateProvider(LocalStorageService localStorageService)
        {
            _localStorageService = localStorageService;
        }
        public override async Task<AuthenticationState> GetAuthenticationStateAsync()
        {
            var token = await _localStorageService.GetItemAsync("authToken");
            var identity = new ClaimsIdentity();

            if (!string.IsNullOrEmpty(token))
            {
                var userRole = await GetUserRoleFromToken(token);

                if (!string.IsNullOrEmpty(userRole))
                {
                    identity = new ClaimsIdentity(new[]
                    {
                        new Claim(ClaimTypes.Role, userRole)
                    }, "jwt");
                }
                else
                {
                    Console.WriteLine("userRole is null");
                }
            }else
            {
                Console.WriteLine("token is null");
            }

            var user = new ClaimsPrincipal(identity);
            return new AuthenticationState(user);
        }

        private Task<string> GetUserRoleFromToken(string token)
        {
            var handler = new JwtSecurityTokenHandler();
            var jwtToken = handler.ReadToken(token) as JwtSecurityToken;

            var roleClaim = jwtToken?.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Role);
            return Task.FromResult(roleClaim?.Value ?? string.Empty);
        }
        public async Task Logout()
        {
            await _localStorageService.RemoveItemAsync("authToken");
            NotifyAuthenticationStateChanged(GetAuthenticationStateAsync());
        }
    }
}
