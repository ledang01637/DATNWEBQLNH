using DATN.Shared;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using Newtonsoft.Json.Linq;
using System;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.WebUtilities;
using System.Collections.Generic;
using DATN.Client.Service;

namespace DATN.Client.Pages
{
    public partial class LoadQR
    {
        private Table table = new Table();
        private List<Table> tables = new List<Table>();
        private QR qR = new QR();

        protected override async Task OnInitializedAsync()
        {
            var uri = Navigation.ToAbsoluteUri(Navigation.Uri);
            if (QueryHelpers.ParseQuery(uri.Query).TryGetValue("n", out var extractedMd5))
            {
                await ProcessMd5Value(extractedMd5);
            }
        }
        private async Task ProcessMd5Value(string md5)
        {
            try
            {
                tables = await httpClient.GetFromJsonAsync<List<Table>>("api/Table/GetTable");

                foreach (var t in tables)
                {
                    var encodedTableNumber = await GenerateMD5Hash(t.TableNumber.ToString());
                    if (encodedTableNumber == md5)
                    {
                        table = t;
                        break;
                    }
                }
                qR.NumberTable = table.TableNumber;
                var response = await httpClient.PostAsJsonAsync("api/AuthJWT/GenerateQrToken", qR);
                if (response.IsSuccessStatusCode)
                {
                    var qRResponse = await response.Content.ReadFromJsonAsync<QRResponse>();

                    if (qRResponse != null && qRResponse.IsSuccessFull)
                    {
                        await _localStorageService.SetItemAsync("n", qRResponse.Token);
                        Navigation.NavigateTo("/");
                    }
                    else
                    {
                        await JS.InvokeVoidAsync("showAlert", "warning", "Lỗi qRResponse is null");
                    }
                }
                else
                {
                    await JS.InvokeVoidAsync("showAlert", "error", "Lỗi Post QR");
                }
            }
            catch(Exception ex)
            {
                await JS.InvokeVoidAsync("showAlert", "error", "Lỗi",ex);
            }
            
        }

        private async Task<string> GenerateMD5Hash(string text)
        {
            return await JS.InvokeAsync<string>("generateMD5Hash", text);
        }

    }
}
