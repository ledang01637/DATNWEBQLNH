using DATN.Shared;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.JSInterop;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DATN.Client.Pages.AdminManager
{
    public partial class ManagerChef
    {
        private HubConnection hubConnection;
        public static List<NoteProdReq> noteProdReqs = new();

        protected override async Task OnInitializedAsync()
        {
            hubConnection = new HubConnectionBuilder()
                .WithUrl(Navigation.ToAbsoluteUri("/ProcessHub"))
                .Build();

            hubConnection.On<string, List<CartDTO>, string>("ReqChef", async (_numTable, carts, note) =>
            {
                var prodReqs = new List<ProdReq>();

                if (carts != null && carts.Count > 0)
                {
                    foreach (var c in carts.Where(a => a.UnitId != 1))
                    {
                        prodReqs.Add(new ProdReq
                        {
                            ProductId = c.ProductId,
                            ProductName = c.ProductName,
                            Quantity = c.Quantity,
                            IsComplete = false
                        });
                    }
                }

                noteProdReqs.Add(new NoteProdReq
                {
                    TableNumber = _numTable,
                    ProdReqs = prodReqs,
                    Note = note
                });

                await _localStorageService.SetAsync("noteProdReqs", noteProdReqs);
                await JS.InvokeVoidAsync("renderFoodList", ".card");
                StateHasChanged();
            });

            await hubConnection.StartAsync();

            noteProdReqs = await _localStorageService.GetAsync<List<NoteProdReq>>("noteProdReqs") ?? new List<NoteProdReq>();
            StateHasChanged();
        }

        public async Task ConfirmRequestAsync(int productId)
        {
            foreach (var note in noteProdReqs)
            {
                var prod = note.ProdReqs.FirstOrDefault(a => a.ProductId == productId);
                if (prod != null)
                {
                    prod.IsComplete = true;
                    break;
                }
            }
            await _localStorageService.SetAsync("noteProdReqs", noteProdReqs);
            StateHasChanged();
        }

        public async ValueTask DisposeAsync()
        {
            if (hubConnection is not null)
            {
                await hubConnection.DisposeAsync();
            }
        }
    }

    public class ProdReq
    {
        public int ProductId { get; set; }
        public string ProductName { get; set; }
        public int Quantity { get; set; }
        public bool IsComplete { get; set; }
    }

    public class NoteProdReq
    {
        public string TableNumber { get; set; }
        public List<ProdReq> ProdReqs { get; set; } = new();
        public string Note { get; set; }
    }
}
