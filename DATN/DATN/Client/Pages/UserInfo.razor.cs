using DATN.Shared;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.JSInterop;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Threading.Tasks;

namespace DATN.Client.Pages
{
    public partial class UserInfo
    {
        protected override async Task OnInitializedAsync()
        {
            var accountType = await CheckTypeAccount();
            if(accountType == null && accountType != "customer") 
            {
                Navigation.NavigateTo("/login");
                return;
            }
        }

        private async Task<string> CheckTypeAccount()
        {
            var token = await _localStorageService.GetItemAsync("authToken");
            if (!string.IsNullOrEmpty(token))
            {
                var handler = new JwtSecurityTokenHandler();

                if (handler.ReadToken(token) is JwtSecurityToken jwtToken)
                {
                    var accountTypeClaim = jwtToken.Claims.FirstOrDefault(c => c.Type.Equals("AccountType"));
                    return accountTypeClaim?.Value;
                }
                else
                {
                    await JS.InvokeVoidAsync("showAlert", "error", "Lỗi", "Vui lòng liên hệ Admin");
                }
            }
            return null;
        }
    }
}
