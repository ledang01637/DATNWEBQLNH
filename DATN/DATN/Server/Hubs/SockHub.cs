using Microsoft.AspNetCore.SignalR;
using System.Threading.Tasks;

namespace DATN.Server.Hubs
{
    public class SockHub : Hub
    {
        public async Task SendMessage(string message)
        {
            await Clients.All.SendAsync("UpdateAdminDashboard", message);
        }
    }
}
