using DATN.Shared;
using Microsoft.JSInterop;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Net.Http.Json;
using System.Threading.Tasks;

namespace DATN.Client.Pages
{
    public partial class VoiceCallCustomer
    {
        private string token;
        private string from;
        private string to = "Manager";

        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            if (firstRender)
            {
                token = await _localStorageService.GetItemAsync("n");
                if (token == null)
                {
                    await JS.InvokeVoidAsync("showAlert", "error", "Token is null");
                    return;
                }
                from = GetTableNumberFromToken(token);
                if(from == null)
                {
                    await JS.InvokeVoidAsync("showAlert", "error", "From is null");
                    return;
                }
                await SetupCall(token, from, to);
                await setupVideo();
            }
        }
        private async Task SetupCall(string token, string from, string to)
        {
            try
            {
                var response = await httpClient.PostAsJsonAsync("api/JwtTokenValidator/ValidateToken/", token);
                if(response.IsSuccessStatusCode)
                {
                    var handler = new JwtSecurityTokenHandler();

                    if (handler.ReadToken(token) is not JwtSecurityToken jsonToken)
                    {
                        await JS.InvokeVoidAsync("showAlert", "error", "Token is invalid");
                    }
                    else
                    {
                        bool isCall = true;
                        await JS.InvokeVoidAsync("setupCall", token, from, to, isCall);
                        await JS.InvokeVoidAsync("layout");
                    }
                }

                
            }
            catch(Exception ex)
            {
                await JS.InvokeVoidAsync("showAlert", "error", ex);
            }
        }
        private async Task setupVideo()
        {
            await JS.InvokeVoidAsync("setupVideo", "btn-answer", "btn-call", "remoteVideo", "localVideo");
        }
        private void EndCall()
        {
            Navigation.NavigateTo("/");
        }
        private static string GetTableNumberFromToken(string token)
        {
            var handler = new JwtSecurityTokenHandler();
            var jwtToken = handler.ReadToken(token) as JwtSecurityToken;
            var userId = jwtToken?.Claims.FirstOrDefault(c => c.Type == "userId");
            return userId?.Value;
        }
    }
}
