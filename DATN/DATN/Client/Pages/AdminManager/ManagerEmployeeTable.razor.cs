﻿using Microsoft.EntityFrameworkCore.Metadata.Internal;
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
        private string from;
        private string to;
        public DotNetObjectReference<ManagerEmployeeTable> dotNetObjectReference;

        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            dotNetObjectReference = DotNetObjectReference.Create(this);
            if (firstRender)
            {
                token = await _localStorageService.GetItemAsync("m");
                from = GetTableNumberFromToken(token);
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
                            bool isCall = false;
                            await JS.InvokeVoidAsync("setupCall", token, from, to, isCall, dotNetObjectReference);
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

        [JSInvokable("EndCall")]
        public void EndCall()
        {
            
        }
        private string GetTableNumberFromToken(string token)
        {
            var handler = new JwtSecurityTokenHandler();
            var jwtToken = handler.ReadToken(token) as JwtSecurityToken;
            var userId = jwtToken?.Claims.FirstOrDefault(c => c.Type == "userId");
            return userId?.Value;
        }

        private async void CallButton(bool isClose)
        {
            await JS.InvokeVoidAsync("callButtonManager", isClose);
        }

    }
}