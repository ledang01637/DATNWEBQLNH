using DATN.Shared;
using Microsoft.JSInterop;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;

namespace DATN.Client.Pages
{
    public partial class Invoice
    {
        private List<Cart> carts = new List<Cart>();
        private decimal Total;
        private decimal Tax = 0.1m;
        private decimal ServiceFee = 0.05m;
        private string numberTable = "";
        protected override async Task OnInitializedAsync()
        {
            carts = await _localStorageService.GetCartItemAsync("historyOrder");

            numberTable = await _localStorageService.GetItemAsync("n");

            if (carts.Count > 0)
            {
                CalculateTotal();
            }
        }
        private void CalculateTotal()
        {
            Total = 0;
            foreach (var item in carts)
            {
                Total += item.Price * item.Quantity;
            }
        }
        private async Task PrintBill()
        {
            var response = await httpClient.PostAsync("api/PrintBill/PrintReceipt", null);
            if (response.IsSuccessStatusCode)
            {
                await JS.InvokeVoidAsync("alert", "In hóa đơn thành công");
            }
            else
            {
                await JS.InvokeVoidAsync("alert", "Lỗi khi in hóa đơn");
            }
        }
        private async Task PrintFromBrowser()
        {
            await JS.InvokeVoidAsync("printInvoicePreview");
        }
    }
}
