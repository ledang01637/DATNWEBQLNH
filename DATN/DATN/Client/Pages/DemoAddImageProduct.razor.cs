using Microsoft.JSInterop;
using System.Net.Http.Json;
using System.Threading.Tasks;

namespace DATN.Client.Pages
{
    public partial class DemoAddImageProduct
    {
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
