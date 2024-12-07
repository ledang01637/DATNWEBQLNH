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
using Microsoft.AspNetCore.SignalR.Client;

namespace DATN.Client.Pages
{
    public partial class LoadQR
    {
        private Table table = new();
        private List<Table> tables = new();
        private QR qR = new();
        private bool isBookTable = false;
        private bool isLoad = false;

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
            isLoad = true;
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
                        int numberTable = GetTableNumberFromToken(qRResponse.Token);

                        var table = await CheckBookTableAsync(numberTable);

                        if(table != null && table.Status.Equals("availableuntil")) 
                        {
                            isBookTable = true;
                            return;
                        }
                        await _localStorageService.SetItemAsync("n", qRResponse.Token);
                        Navigation.NavigateTo("/");
                    }
                    else
                    {
                        await JS.InvokeVoidAsync("showAlert", "error", "Lỗi","Không thể đọc QR");
                    }
                }
                else
                {
                    await JS.InvokeVoidAsync("showAlert", "error", "Lỗi","Không thể kiểm tra QR");
                }
            }
            catch(Exception ex)
            {
                var query = $"[C#] fix error bằng tiếng việt: {ex.Message}";
                await JS.InvokeVoidAsync("openChatGPT", query);
                await JS.InvokeVoidAsync("showAlert", "error", "Lỗi","Lỗi không xác định");
            }
            finally
            {
                isLoad = false;
            }
            
        }

        private async Task<string> GenerateMD5Hash(string text)
        {
            return await JS.InvokeAsync<string>("generateMD5Hash", text);
        }

        private static int GetTableNumberFromToken(string token)
        {
            var handler = new JwtSecurityTokenHandler();
            var jwtToken = handler.ReadToken(token) as JwtSecurityToken;
            var userId = jwtToken?.Claims.FirstOrDefault(c => c.Type == "userId");
            return int.Parse(userId?.Value);
        }
        private async Task<Table> CheckBookTableAsync(int numberTable)
        {
            try
            {
                var table = await httpClient.GetFromJsonAsync<Table>($"api/Table/GetTableByNumber?numberTable={numberTable}");

                return table;
            }
            catch(Exception ex)
            {
                var query = $"[C#] fix error bằng tiếng việt: {ex.Message}";
                await JS.InvokeVoidAsync("openChatGPT", query);
                await JS.InvokeVoidAsync("showAlert", "error", "Lỗi", "Lỗi khi lấy dữ liệu bàn");
                return null;
            }

        }
    }
}
