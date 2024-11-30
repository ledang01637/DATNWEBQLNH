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
        public static List<NoteProdReq> noteProdReqs = new ();
        public DotNetObjectReference<ManagerChef> dotNetObjectReference;

        protected override async Task OnInitializedAsync()
        {
            dotNetObjectReference = DotNetObjectReference.Create(this);

            hubConnection = new HubConnectionBuilder()
                .WithUrl(Navigation.ToAbsoluteUri("/ProcessHub"))
                .Build();

            hubConnection.On<string, List<CartDTO>, string>("ReqChef", async (_numTable, carts, note) =>
            {
                var prodReqs = carts.Select(c => new ProdReq
                {
                    ProductId = c.ProductId,
                    ProductName = c.ProductName,
                    Quantity = c.Quantity,
                    CompletedQuantity = 0,
                    UnitName = c.UnitName,
                }).ToList();

                await HandleProdReqAsync(_numTable, prodReqs, note);
                StateHasChanged();
            });

            await hubConnection.StartAsync();

            hubConnection.Closed += async (error) =>
            {
                Console.WriteLine($"Connection lost: {error?.Message}");
                await Task.Delay(1000);
                try
                {
                    await hubConnection.StartAsync();
                    Console.WriteLine("Reconnected to the server.");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Reconnection failed: {ex.Message}");
                }
            };

            NoteProdReq.noteProdReqs = noteProdReqs = await _localStorageService.GetAsync<List<NoteProdReq>>("noteProdReqs") ?? new List<NoteProdReq>();

            StateHasChanged();
        }


        public async Task HandleProdReqAsync(string tableNumber, List<ProdReq> prodReqs, string note)
        {
            try
            {
                var filteredProdReqs = prodReqs.Where(a => a.UnitName.ToLower() != "chai").ToList();

                if (filteredProdReqs.Any())
                {
                    foreach (var prod in filteredProdReqs)
                    {
                        AddOrUpdateProdReq(tableNumber, prod, note);
                    }

                    await _localStorageService.SetAsync("noteProdReqs", NoteProdReq.noteProdReqs);

                    await JS.InvokeVoidAsync("renderFoodList", ".card", dotNetObjectReference);
                    StateHasChanged();
                }
            }
            catch (Exception ex)
            {
                await JS.InvokeVoidAsync("showAlert", "error", "Lỗi","Vui lòng liên hệ admin: ", ex.Message);
            }
            
        }

        public void AddOrUpdateProdReq(string tableNumber, ProdReq newProdReq, string note)
        {
            var notePR = NoteProdReq.noteProdReqs.FirstOrDefault(n => n.TableNumber == tableNumber);

            if (notePR == null)
            {
                noteProdReqs.Add(new NoteProdReq
                {
                    TableNumber = tableNumber,
                    ProdReqs = new List<ProdReq> { newProdReq },
                    Note = note
                });
            }
            else
            {
                var existingProd = notePR.ProdReqs.FirstOrDefault(p => p.ProductId == newProdReq.ProductId);

                if (existingProd != null)
                {
                    existingProd.Quantity += newProdReq.Quantity;
                }
                else
                {
                    notePR.ProdReqs.Add(newProdReq);
                }

                if (!string.IsNullOrWhiteSpace(note))
                {
                    notePR.Note = $"{notePR.Note} | {note}".TrimStart('|');

                }
            }
        }

        public async Task ConfirmRequestAsync(string tableNumber, int productId, int completedQuantity)
        {
            var notePR = NoteProdReq.noteProdReqs.FirstOrDefault(n => n.TableNumber == tableNumber);

            if (notePR != null)
            {
                var prod = notePR.ProdReqs.FirstOrDefault(p => p.ProductId == productId);
                if (prod != null)
                {
                    prod.CompletedQuantity += completedQuantity;
                    if (prod.CompletedQuantity > prod.Quantity)
                    {
                        prod.CompletedQuantity = prod.Quantity;
                    }
                }
                bool allCompleted = notePR.ProdReqs.All(p => p.IsComplete);
                await JS.InvokeVoidAsync("checkCompleteProd", allCompleted, dotNetObjectReference);
            }

            await _localStorageService.SetAsync("noteProdReqs", noteProdReqs);

            StateHasChanged();
        }

        private static bool IsCompleteAllProd(string tableNumber)
        {
            var notePR = NoteProdReq.noteProdReqs.FirstOrDefault(n => n.TableNumber == tableNumber);

            if (notePR == null || notePR.ProdReqs.Count == 0)
            {
                return true; 
            }
            return notePR.ProdReqs.All(p => p.IsComplete);
        }


        [JSInvokable]
        public async Task RemoveNoteProdReq(string tableNumber)
        {
            if (noteProdReqs != null && noteProdReqs.Any())
            {
                NoteProdReq.noteProdReqs = noteProdReqs = noteProdReqs.Where(req => req.TableNumber != tableNumber).ToList();

                await _localStorageService.SetAsync("noteProdReqs", noteProdReqs);

                if(IsCompleteAllProd(tableNumber))
                {

                }
                else
                {
                    await JS.InvokeVoidAsync("showAlert", "warning", "Thông báo", "Còn món ăn chưa làm bàn số: " + tableNumber);
                }
                StateHasChanged();
            }
        }


        public async ValueTask DisposeAsync()
        {
            if (hubConnection is not null)
            {
                await hubConnection.DisposeAsync();
            }
        }
        public void Dispose()
        {
            dotNetObjectReference?.Dispose();
            dotNetObjectReference = null;
        }
    }
}
