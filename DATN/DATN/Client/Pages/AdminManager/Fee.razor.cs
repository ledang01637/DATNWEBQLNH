using Microsoft.JSInterop;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;

namespace DATN.Client.Pages.AdminManager
{
    public partial class Fee
    {
        private decimal TableFee;
        private async Task HandleSubmit()
        {

            if (TableFee <= 0)
            {
                await JS.InvokeVoidAsync("showAlert", "warning", "Thông báo", "Phí đặt bàn phải lớn hơn 0.");
                return;
            }

            var response = await httpClient.PostAsJsonAsync("api/Fee/post-fee", TableFee);

            var result = await response.Content.ReadAsStringAsync();

            if (response.IsSuccessStatusCode)
            {
                await JS.InvokeVoidAsync("showAlert", "success", "Thông báo", result);
                return;
            }
            else
            {
                if (response.StatusCode == System.Net.HttpStatusCode.BadRequest)
                {
                    await JS.InvokeVoidAsync("showAlert", "error", "Lỗi", result);
                }
                else
                {
                    await JS.InvokeVoidAsync("showAlert", "error", "Lỗi", result);
                }
            }
        }
    }
}
