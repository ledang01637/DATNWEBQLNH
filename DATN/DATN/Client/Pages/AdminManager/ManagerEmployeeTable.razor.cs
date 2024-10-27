using Microsoft.EntityFrameworkCore.Metadata.Internal;
using System.IdentityModel.Tokens.Jwt;
using System.Net.Http;
using System.Threading.Tasks;
using System;
using Microsoft.JSInterop;
using System.Net.Http.Json;
using System.Linq;
using DATN.Shared;
using System.Collections.Generic;
using Microsoft.AspNetCore.SignalR.Client;

namespace DATN.Client.Pages.AdminManager
{
    public partial class ManagerEmployeeTable
    {
        private List<Table> tables = new();
        private List<Floor> floors = new();
        private List<Product> products = new();
        private HubConnection hubConnection;
        public DotNetObjectReference<ManagerEmployeeTable> dotNetObjectReference;
        public static List<RequestCustomer> requests = new();
         
        private Dictionary<int, ButtonVisibility> tableButtonVisibility = new();
        private List<int> numtables = new();
        private Dictionary<int, CartNote> cartsByTable = new();
        private CartNote _cartNote = new();



        private decimal TotalAmount = 0;
        private bool IsUsing = false;
        private int selectedTableNumber;
        private static int nextRequestId = 1;
        private string getMessage;
        private string getReq;
        private string token;
        private string from;
        private string to;


        protected override async Task OnInitializedAsync()
        {
            hubConnection = new HubConnectionBuilder()
                .WithUrl(Navigation.ToAbsoluteUri("/ProcessHub"))
                .Build();

            hubConnection.On<string, int>("RequidTable", (message, numberTable) =>
            {
                RequestCustomer.requestCustomers.Add(new RequestCustomer()
                {
                    RequestId = nextRequestId++,
                    TableNumbe = numberTable,
                    RequestText = message,
                    IsCompleted = false
                });;
                requests = RequestCustomer.requestCustomers;
                StateHasChanged();
            });

            hubConnection.On<string, List<CartDTO>, string>("UpdateTable", (numTable, carts, note) =>
            {
                getMessage = numTable;

                if (int.TryParse(getMessage, out int tableNumber))
                {
                    if (cartsByTable.TryGetValue(tableNumber, out var existingCartNote))
                    { 
                        existingCartNote.Note = MergeNotes(existingCartNote.Note, note);

                        foreach (var newItem in carts)
                        {
                            var existingItem = existingCartNote.CartDTOs.FirstOrDefault(item => item.ProductId == newItem.ProductId);
                            if (existingItem != null)
                            {
                                existingItem.Quantity += newItem.Quantity;
                            }
                            else
                            {
                                existingCartNote.CartDTOs.Add(newItem);
                            }
                        }
                        _cartNote = existingCartNote;
                    }
                    else
                    {
                        _cartNote = new CartNote
                        {
                            CartDTOs = carts,
                            PreviousCartDTOs = new(),
                            Note = note
                        };
                        cartsByTable[tableNumber] = _cartNote;
                    }

                    if (!numtables.Contains(tableNumber))
                    {
                        numtables.Add(tableNumber);
                    }
                    InitializeButtonVisibility(tableNumber);
                }

                StateHasChanged();
            });

            hubConnection.On<string>("ReqMessage", (message) =>
            {
                getReq = message;
                StateHasChanged();
            });

            await hubConnection.StartAsync();
            string Username = await _localStorageService.GetItemAsync("userName");

            var response = await httpClient.PostAsJsonAsync("api/Voice/post-message", Username);
            if(!response.IsSuccessStatusCode)
            {
                await JS.InvokeVoidAsync("showAlert", "error","Lỗi","Không post được value");
            }

            await LoadAll();
        }
        private void ShowModalForTable(int numberTable)
        {
            selectedTableNumber = numberTable;
            if (cartsByTable.TryGetValue(numberTable, out var existingCartNote))
            {
                _cartNote = new CartNote
                {
                    CartDTOs = existingCartNote.CartDTOs.ToList(),   
                    PreviousCartDTOs = existingCartNote.PreviousCartDTOs.ToList(),
                    Note = existingCartNote.Note
                };
            }
            else
            {
                _cartNote = new CartNote
                {
                    CartDTOs = new(),
                    PreviousCartDTOs = new(),
                    Note = string.Empty
                };
            }
            StateHasChanged();
        }


        private async void ConfirmOrder()
        {
            IsUsing = true;
            if (_cartNote.CartDTOs is null || !_cartNote.CartDTOs.Any())
            {
                await JS.InvokeVoidAsync("showAlert", "warning", "Không có món mới");
                return;
            }

            if (cartsByTable.TryGetValue(selectedTableNumber, out var existingCartNote))
            {

                foreach (var newItem in _cartNote.CartDTOs)
                {
                    var existingItem = existingCartNote.PreviousCartDTOs.FirstOrDefault(item => item.ProductId == newItem.ProductId);
                    if (existingItem != null)
                    {
                        existingItem.Quantity += newItem.Quantity;
                    }
                    else
                    {
                        existingCartNote.PreviousCartDTOs.Add(new CartDTO
                        {
                            ProductId = newItem.ProductId,
                            ProductName = newItem.ProductName,
                            Quantity = newItem.Quantity,
                            Price = newItem.Price
                        });
                    }
                }
                existingCartNote.CartDTOs = new List<CartDTO>(); ;

            }
            else
            {
                cartsByTable[selectedTableNumber] = new CartNote
                {
                    PreviousCartDTOs = new List<CartDTO>(_cartNote.CartDTOs),
                    CartDTOs = new List<CartDTO>(),
                    Note = _cartNote.Note
                };
                existingCartNote.CartDTOs = new List<CartDTO>();
            }

            await JS.InvokeVoidAsync("closeModal", "tableModal");
            await JS.InvokeVoidAsync("showAlert", "success", "Đã gửi đầu bếp");
            InitializeButtonVisibility(selectedTableNumber);
            StateHasChanged();
        }


        private async void ProcessPayment()
        {
            await JS.InvokeVoidAsync("closeModal", "tableModal");
            await JS.InvokeVoidAsync("showAlert", "success", "Đã thanh toán");
            IsUsing = false;
            cartsByTable.Remove(selectedTableNumber);
            numtables.Remove(selectedTableNumber);
            InitializeButtonVisibility(selectedTableNumber);
            StateHasChanged();
        }

        private async void CancelOrder()
        {
            
        }


        private string GetTableColor(int tableNumber)
        {

            if (!string.IsNullOrEmpty(getReq) && numtables.Contains(tableNumber))
            {
                return "#FFD700";
            }
            else if (IsUsing && numtables.Contains(tableNumber))
            {
                return "#FFA500";
            }
            else if (!string.IsNullOrEmpty(getMessage) && numtables.Contains(tableNumber))
            {
                return "#ADD8E6";

            }
            else
            {
                return "#32CD32";
            }

        }
        private void InitializeButtonVisibility(int tableNumber)
        {
            if (!tableButtonVisibility.ContainsKey(tableNumber))
            {
                tableButtonVisibility[tableNumber] = new ButtonVisibility();
            }

            var visibility = tableButtonVisibility[tableNumber];

            if (cartsByTable.TryGetValue(tableNumber, out var cartNote) && cartNote.CartDTOs.Any())
            {
                visibility.IsConfirmVisible = true;
                visibility.IsCheckoutVisible = false;
            }
            else if (numtables.Contains(tableNumber) && IsUsing)
            {
                visibility.IsConfirmVisible = false;
                visibility.IsCheckoutVisible = true;
            }
            else
            {
                visibility.IsConfirmVisible = false;
                visibility.IsCheckoutVisible = false;
            }

            StateHasChanged();
        }




        private async Task LoadAll()
        {
            try
            {
                tables = await httpClient.GetFromJsonAsync<List<Table>>("api/Table/GetTable");
                if (tables.Any())
                {
                    tables = tables.Where(a => !a.IsDeleted).ToList();
                }
                floors = await httpClient.GetFromJsonAsync<List<Floor>>("api/Floor/GetFloor");
                if (floors.Any())
                {
                    floors = floors.Where(a => !a.IsDeleted).ToList();
                }
                products = await httpClient.GetFromJsonAsync<List<Product>>("api/Product/GetProduct");
                if (products.Any())
                {
                    products = products.Where(a => !a.IsDeleted).ToList();
                }
            }
            catch (Exception ex)
            {
                await HandleError(ex);
            }
        }



        private async Task HandleError(Exception ex)
        {
            var query = $"[C#] fix error: {ex.Message}";
            await JS.InvokeVoidAsync("openChatGPT", query);
            Console.WriteLine($"{ex.Message}");
        }

        private string MergeNotes(string existingNote, string newNote)
        {
            if (string.IsNullOrEmpty(existingNote))
            {
                return newNote;
            }
            if (string.IsNullOrEmpty(newNote))
            {
                return existingNote;
            }
            return $"{existingNote}; {newNote}";
        }

        #region VoiceCall
        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            dotNetObjectReference = DotNetObjectReference.Create(this);
            if (firstRender)
            {
                token = await _localStorageService.GetItemAsync("m");
                from = GetTableNumberFromToken(token);

                await SetupCall(token, from, to);
                await setupVideo();
            }
        }
        private async Task SetupCall(string token, string from, string to)
        {
            try
            {
                var response = await httpClient.PostAsJsonAsync("api/JwtTokenValidator/ValidateToken", token);
                if (response.IsSuccessStatusCode)
                {
                    var handler = new JwtSecurityTokenHandler();

                    if (handler.ReadToken(token) is not JwtSecurityToken jsonToken)
                    {
                        await JS.InvokeVoidAsync("showAlert", "error", "Token is invalid");
                    }
                    else
                    {
                        if (token != null)
                        {
                            bool isCall = false;
                            await JS.InvokeVoidAsync("setupCall", token, from, to, isCall, dotNetObjectReference);
                            await JS.InvokeVoidAsync("layout");
                        }
                        else
                        {
                            await JS.InvokeVoidAsync("showAlert", "warning", "Token is null");
                        }
                    }
                }


            }
            catch (Exception ex)
            {
                await JS.InvokeVoidAsync("showAlert", "error", ex);
            }


        }
        private async Task setupVideo()
        {
            await JS.InvokeVoidAsync("setupVideo", "btn-answer", "btn-call", "remoteVideo", "localVideo");
        }

        [JSInvokable("EndCall")]
        public void EndCall()
        {

        }
        private string GetTableNumberFromToken(string token)
        {
            var handler = new JwtSecurityTokenHandler();
            var jwtToken = handler.ReadToken(token) as JwtSecurityToken;
            var userId = jwtToken?.Claims.FirstOrDefault(c => c.Type == "userId");
            return userId?.Value.ToLower();
        }

        private async void CallButton(bool isClose)
        {
            await JS.InvokeVoidAsync("callButtonManager", isClose);
        }
        #endregion

        private static void ConfirmRequest(int RequestId)
        {
            var a = requests.FirstOrDefault(a => a.RequestId == RequestId);
            if(a is not null)
            {
                a.IsCompleted = true;
            }
        }
        public void Dispose()
        {
            dotNetObjectReference?.Dispose();
        }
        public async ValueTask DisposeAsync()
        {
            if (hubConnection is not null)
            {
                await hubConnection.DisposeAsync();
            }
        }

    }
    public class CartNote
    {
        public List<CartDTO> CartDTOs { get; set; }
        public List<CartDTO> PreviousCartDTOs { get; set; }
        public string Note {  get; set; }
    }
    public class ButtonVisibility
    {
        public bool IsConfirmVisible { get; set; }
        public bool IsCheckoutVisible { get; set; }
    }
}
