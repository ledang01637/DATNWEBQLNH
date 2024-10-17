using DATN.Shared;
using Microsoft.AspNetCore.SignalR;
using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Threading.Tasks;

namespace DATN.Server.Hubs
{
    public class SockHub : Hub
    {
        public async Task SendTable(string message, List<CartDTO> carts)
        {
            await Clients.All.SendAsync("UpdateTable", message, carts);
        }
        public async Task SendMessage(string message)
        {
            await Clients.All.SendAsync("UpdateAdminDashboard", message);
        }
        public async Task SendOrderUpdate(Order order)
        {
            if (order == null) { Console.WriteLine("Order is null"); return; }
            await Clients.All.SendAsync("UpdateOrder", order);
        }

    }
}
