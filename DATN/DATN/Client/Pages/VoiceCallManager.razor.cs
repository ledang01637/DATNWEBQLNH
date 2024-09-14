using Microsoft.JSInterop;
using System.Threading.Tasks;

namespace DATN.Client.Pages
{
    public partial class VoiceCallManager
    {
        private string token = "eyJjdHkiOiJzdHJpbmdlZS1hcGk7dj0xIiwidHlwIjoiSldUIiwiYWxnIjoiSFMyNTYifQ.eyJqdGkiOiJTSy4wLjZoSHpGNDlFTHlQMDlUc0Q4VTAwbENERlhEMTdQeS0xNzI2MzAxMzkxIiwiaXNzIjoiU0suMC42aEh6RjQ5RUx5UDA5VHNEOFUwMGxDREZYRDE3UHkiLCJleHAiOjE3Mjg4OTMzOTEsInVzZXJJZCI6Im1hbmFnZXIifQ.BesiMbOrekjh40schU6nRT1dsTmKQC6uF8YLIiQ0-vk";
        private string from = "manager";
        private string to = "customer";
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
