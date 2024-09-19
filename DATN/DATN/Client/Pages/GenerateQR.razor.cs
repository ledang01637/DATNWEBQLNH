using DATN.Shared;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using System;
using System.Net.Http.Json;
using Microsoft.JSInterop;
using static System.Net.WebRequestMethods;
using System.Data.Common;
using Microsoft.AspNetCore.Components;
using Newtonsoft.Json.Linq;

namespace DATN.Client.Pages
{
    public partial class GenerateQR
    {
        private List<Table> tables = new List<Table>();
        private QR qrModel = new QR();
        protected override async Task OnInitializedAsync()
        {
            qrModel.Url = "https://localhost:44328/";
            await LoadAll();
        }
        private async Task LoadAll()
        {
            try
            {
                tables = await httpClient.GetFromJsonAsync<List<Table>>("api/Table/GetTable");
                
            }
            catch (Exception ex)
            {
                var query = $"[C#] fix error: {ex.Message}";
                await JS.InvokeVoidAsync("openChatGPT", query);
            }
        }
        private async Task GenerateQrCode()
        {

            var response = await httpClient.PostAsJsonAsync("api/AuthJWT/GenerateQrToken", qrModel);
            var result = await response.Content.ReadFromJsonAsync<QRResponse>();

            if (result != null)
            {
                var encodedToken = Uri.EscapeDataString(result.Token);
                var urlCode = $"{qrModel.Url}demoIndex?token={encodedToken}";

                await _localStorageService.SetItemAsync("ss", urlCode);
                await JS.InvokeVoidAsync("clearQrCode");
                await JS.InvokeVoidAsync("generateQrCode", urlCode);
            }
        }
    }
}
