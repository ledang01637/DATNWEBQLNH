using Microsoft.JSInterop;
using System.Threading.Tasks;

namespace DATN.Client.Pages
{
    public partial class VoiceCallCustomer
    {
        private string token = "eyJjdHkiOiJzdHJpbmdlZS1hcGk7dj0xIiwidHlwIjoiSldUIiwiYWxnIjoiSFMyNTYifQ.eyJqdGkiOiJTSy4wLjZoSHpGNDlFTHlQMDlUc0Q4VTAwbENERlhEMTdQeS0xNzI2MzAxMzY1IiwiaXNzIjoiU0suMC42aEh6RjQ5RUx5UDA5VHNEOFUwMGxDREZYRDE3UHkiLCJleHAiOjE3Mjg4OTMzNjUsInVzZXJJZCI6ImN1c3RvbWVyIn0.H34y6OkC6S_BZAhjTW073WHL-7rGqBeax1j9kUSnk-8  ";
        private string from = "customer";
        private string to = "manager";
        private async Task SetupCall()
        {
            await JS.InvokeVoidAsync("setupCall", token, from, to);
        }
        private async Task setupVideo()
        {
            await JS.InvokeVoidAsync("setupVideo", "answerCallButton", "callButton", "remoteVideo", "localVideo");
        }

        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            if (firstRender)
            {
                await SetupCall();
                await setupVideo();
            }
        }
    }
}
