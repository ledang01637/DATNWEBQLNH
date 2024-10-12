using Microsoft.EntityFrameworkCore.Metadata.Internal;
using System.IdentityModel.Tokens.Jwt;
using System.Net.Http;
using System.Threading.Tasks;
using System;
using Microsoft.JSInterop;
using System.Net.Http.Json;
using System.Linq;

namespace DATN.Client.Pages.AdminManager
{
    public partial class ManagerEmployeeTable
    {
        private string token;
        private string from = "danglh";
        private string to;

        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            if (firstRender)
            {
                token = await _localStorageService.GetItemAsync("m");
                to = GetTableNumberFromToken(token);
                await SetupCall(token, from, to);
                await setupVideo();
            }
        }
        private async Task SetupCall(string token, string from, string to)
        {
            try
            {
                var response = await httpClient.PostAsJsonAsync("api/JwtTokenValidator/ValidateToken", token);
                if (response.IsSuccessStatusCode)
                {
                    var handler = new JwtSecurityTokenHandler();
                    var jsonToken = handler.ReadToken(token) as JwtSecurityToken;

                    if (jsonToken == null)
                    {
                        await JS.InvokeVoidAsync("showAlert", "error", "Token is invalid");
                    }
                    else
                    {
                        if (token != null)
                        {
                            await JS.InvokeVoidAsync("setupCall", token, from, to);
                            await JS.InvokeVoidAsync("layout");
                        }
                        else
                        {
                            await JS.InvokeVoidAsync("showAlert", "warning", "Token is null");
                        }
                    }
                }


            }
            catch (Exception ex)
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
        private string GetTableNumberFromToken(string token)
        {
            var handler = new JwtSecurityTokenHandler();
            var jwtToken = handler.ReadToken(token) as JwtSecurityToken;
            var tableNumberClaim = jwtToken?.Claims.FirstOrDefault(c => c.Type == "tableNumber");
            return tableNumberClaim?.Value;
        }

    }
}
