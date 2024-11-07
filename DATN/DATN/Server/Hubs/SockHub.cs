using DATN.Shared;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Caching.Distributed;
using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Threading.Tasks;

namespace DATN.Server.Hubs
{
    public class SockHub : Hub
    {
        public override async Task OnConnectedAsync()
        {
            await base.OnConnectedAsync();
        }

        public override Task OnDisconnectedAsync(Exception exception)
        {
            return base.OnDisconnectedAsync(exception);
        }

        public async Task SendTable(string message, List<CartDTO> carts, string note)
        {
            await Clients.All.SendAsync("UpdateTable", message, carts, note);
        }
        public async Task SendChef(string message, List<CartDTO> carts, string note)
        {
            await Clients.All.SendAsync("ReqChef", message, carts, note);
        }

        public async Task SendMessageTable(string message, int numberTable)
        {
            await Clients.All.SendAsync("RequidTable", message, numberTable);
        }
        public async Task SendMessage(string message)
        {
            await Clients.All.SendAsync("ReqMessage", message);
        }
        public async Task SendPay(string message, int numberTable, int orderId)
        {
            await Clients.All.SendAsync("ReqPay", message, numberTable, orderId);
        }
        public async Task SendOrderUpdate(Order order)
        {
            if (order == null) { Console.WriteLine("Order is null"); return; }
            await Clients.All.SendAsync("UpdateOrder", order);
        }

    }
}
