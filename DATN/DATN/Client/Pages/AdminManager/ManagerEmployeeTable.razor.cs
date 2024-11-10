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
using System.Text.Json;
using System.Xml;
using System.Xml.Linq;
using System.Dynamic;

namespace DATN.Client.Pages.AdminManager
{
    public partial class ManagerEmployeeTable
    {
        private List<Table> tables = new();
        private List<Floor> floors = new();
        private List<Product> products = new();
        private List<Order> orders = new();
        private List<CallInfo> callInfos = new();
        private Order order = new();
        private Order orderIncludeItem = new();
        private Employee employee = new();
        private HubConnection hubConnection;
        public DotNetObjectReference<ManagerEmployeeTable> dotNetObjectReference;
        public static List<RequestCustomer> requests = new();
        public Customer customer = new();

        private Dictionary<int, ButtonVisibility> tableButtonVisibility = new();
        private Dictionary<int, ColorTable> tableColorsCache = new();
        private Dictionary<int, CartNote> cartsByTable = new();
        private List<int> numtables = new();
        private CartNote _cartNote = new();

        private decimal TotalAmount = 0;
        private bool IsUsing = false;
        private int selectedTableNumber;
        private static int nextRequestId = 1;
        private string numberTable;
        private string messagePay;
        private int orderIdPay;
        private int numberTablePay;
        private string token;
        private string from;
        private string to;

        protected override async Task OnInitializedAsync()
        {
            hubConnection = new HubConnectionBuilder()
                .WithUrl(Navigation.ToAbsoluteUri("/ProcessHub"))
                .Build();

            await SetupHubEvents();
            await hubConnection.StartAsync();

            string username = await _localStorageService.GetItemAsync("userName");
            var response = await httpClient.PostAsJsonAsync("api/Voice/post-message", username);

            if (!response.IsSuccessStatusCode)
            {
                await JS.InvokeVoidAsync("showAlert", "error", "Lỗi", "Vui lòng liên hệ Admin!");
            }

            await GetLocalStorageAsync();
            await LoadAll();
        }

        private async Task GetLocalStorageAsync()
        {
            numtables = await _localStorageService.GetListAsync<int>("numtables") ?? new List<int>();
            requests = await _localStorageService.GetListAsync<RequestCustomer>("requests") ?? new List<RequestCustomer>();
            _cartNote = await _localStorageService.GetAsync<CartNote>("_cartNote") ?? new CartNote();
            cartsByTable = await _localStorageService.GetDictionaryAsync<int, CartNote>("cartsByTable") ?? new Dictionary<int, CartNote>();
            tableButtonVisibility = await _localStorageService.GetDictionaryAsync<int, ButtonVisibility>("tableButtonVisibility") ?? new Dictionary<int, ButtonVisibility>();
            tableColorsCache = await _localStorageService.GetDictionaryAsync<int, ColorTable>("tableColorsCache") ?? new Dictionary<int, ColorTable>();
        }

        // HubEvents
        private Task SetupHubEvents()
        {
            hubConnection.On<string, int>("RequidTable", async (message, numberTable) =>
            {
                var requestCustomer = new RequestCustomer()
                {
                    RequestId = nextRequestId++,
                    TableNumbe = numberTable,
                    RequestText = message,
                    Time = DateTime.Now,
                    IsCompleted = false
                };

                requests.Add(requestCustomer);
                await _localStorageService.SetListAsync("requests", requests);
                await InvokeAsync(StateHasChanged);
            });

            hubConnection.On<string, List<CartDTO>, string>("UpdateTable", async (_numTable, carts, note) =>
            {
                numberTable = _numTable;

                if (int.TryParse(numberTable, out int tableNumber))
                {
                    await UpdateCartNoteAsync(tableNumber, carts, note);
                    await InitializeButtonVisibilityAsync(tableNumber);
                    await GetTableColorAsync(tableNumber);
                    await InvokeAsync(StateHasChanged);
                }
            });

            hubConnection.On<string, int, int, int>("ReqPay", async (message, _numberTable, _orderId, _customerId) =>
            {
                messagePay = message;
                numberTablePay = _numberTable;
                orderIdPay = _orderId;
                order = await GetOrderInvoice(orderIdPay) ?? new Order();
                customer = await GetCustomerById(_customerId) ?? new Customer();
                await GetTableColorAsync(numberTablePay);
            });

            return Task.CompletedTask;
        }



        //InitLoad
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

                orders = await httpClient.GetFromJsonAsync<List<Order>>("api/Order/GetOrderLstInclude");

                if (orders != null && orders.Count > 0)
                {
                    orders = orders.Where(a => !a.IsDeleted).ToList();
                }
                var accountId = await CheckTypeAccountId();
                if (accountId != null)
                {
                    employee = await GetEmployeeByAccountId(int.Parse(accountId));
                    if(employee == null || employee.EmployeeId <= 0 )
                    {
                        await JS.InvokeVoidAsync("showAlert", "warning","Thông báo","Vui lòng đăng nhập bằng tài khoản nhân viên");
                        
                        await Task.Delay(1000);
                        Navigation.NavigateTo("/login-admin");
                        return;
                    }
                }

            }
            catch (Exception ex)
            {
                await HandleError(ex);
            }
        }
        private async Task UpdateCartNoteAsync(int tableNumber, List<CartDTO> carts, string note)
        {
            if (!cartsByTable.TryGetValue(tableNumber, out var existingCartNote))
            {
                existingCartNote = new CartNote { CartDTOs = new List<CartDTO>(), PreviousCartDTOs = new(), Note = note };
                cartsByTable[tableNumber] = existingCartNote;
            }
            else
            {
                existingCartNote.Note = MergeNotes(existingCartNote.Note, note);
            }

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

            if (!numtables.Contains(tableNumber))
            {
                numtables.Add(tableNumber);
                await _localStorageService.SetListAsync("numtables", numtables);
            }
            IsUsing = true;
            await _localStorageService.SetAsync("_cartNote", existingCartNote);
            await _localStorageService.SetDictionaryAsync("cartsByTable", cartsByTable);
        }
        //Modal
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

        private async Task ConfirmOrder()
        {
            try
            {
                _cartNote = await _localStorageService.GetAsync<CartNote>("_cartNote") ?? new CartNote();

                if (_cartNote.CartDTOs is null || !_cartNote.CartDTOs.Any())
                {
                    await JS.InvokeVoidAsync("showAlert", "warning", "Không có món mới");
                    return;
                }
                cartsByTable = await _localStorageService.GetDictionaryAsync<int, CartNote>("cartsByTable");

                if (cartsByTable is null) { await JS.InvokeVoidAsync("showAlert", "error", "Lỗi", "Vui lòng liên hệ Admin!"); return; }

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
                    if (hubConnection is not null && hubConnection.State == HubConnectionState.Connected)
                    {
                        await hubConnection.SendAsync("SendChef", selectedTableNumber.ToString(), _cartNote.CartDTOs, _cartNote.Note);
                    }
                    else
                    {
                        await JS.InvokeVoidAsync("alert", "Không thể kết nối tới server!");
                        return;
                    }
                    existingCartNote.CartDTOs = new List<CartDTO>();
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
                

                IsUsing = true;
                numberTable = null;
                await _localStorageService.SetAsync("_cartNote", existingCartNote);
                await _localStorageService.SetDictionaryAsync("cartsByTable", cartsByTable);
                await GetTableColorAsync(selectedTableNumber);

                await JS.InvokeVoidAsync("closeModal", "tableModal");
                await JS.InvokeVoidAsync("showAlert", "success", "Đã gửi đầu bếp");
                await InitializeButtonVisibilityAsync(selectedTableNumber);
                tableButtonVisibility = await _localStorageService.GetDictionaryAsync<int, ButtonVisibility>("tableButtonVisibility") ?? new Dictionary<int, ButtonVisibility>();
                StateHasChanged();
            }
            catch(Exception ex)
            {
                await JS.InvokeVoidAsync("showAlert", "error", "Lỗi","Vui lòng gọi Admin: " + ex);
            }

        }

        private async void ProcessPayment()
        {
            if (string.IsNullOrEmpty(messagePay))
            {
                await JS.InvokeVoidAsync("showAlert", "warning", "Thông báo","Khách hàng chưa yêu cầu thanh toán");
                return;
            }

            cartsByTable = await _localStorageService.GetDictionaryAsync<int, CartNote>("cartsByTable");

            if (cartsByTable is null) { await JS.InvokeVoidAsync("showAlert", "error", "Lỗi", "Vui lòng liên hệ Admin!"); return; }

            if (cartsByTable.TryGetValue(selectedTableNumber, out var existingCartNote))
            {
                cartsByTable.Remove(selectedTableNumber);
                IsUsing = false;
            }
            else
            {
                cartsByTable[selectedTableNumber] = new CartNote
                {
                    PreviousCartDTOs = new List<CartDTO>(),
                    CartDTOs = new List<CartDTO>(),
                    Note = _cartNote.Note
                };
                existingCartNote.CartDTOs = new List<CartDTO>();
                existingCartNote.PreviousCartDTOs = new List<CartDTO>();

            }

            if (order != null && order.OrderId > 0)
            {
                order.Status = "Đã thanh toán";
                order.EmployeeId = employee.EmployeeId;
                order.TotalAmount = TotalAmount;
                order.Note = _cartNote.Note;

                if(customer != null && customer.CustomerId > 0)
                {
                    await SaveRewarPointes(order);
                }

                var response = await httpClient.PutAsJsonAsync($"api/Order/{order.OrderId}", order);

                if (!response.IsSuccessStatusCode) { await JS.InvokeVoidAsync("showAlert", "error", "Lỗi", "Vui lòng liên hệ Admin update order"); return; }


                TotalAmount = 0;
                IsUsing = false;
                messagePay = null;
                orderIdPay = 0;
                numberTablePay = 0;
                numberTable = null;

                await _localStorageService.SetDictionaryAsync("cartsByTable", cartsByTable);
                await JS.InvokeVoidAsync("closeModal", "tableModal");
                await JS.InvokeVoidAsync("closeModal", "invoiceModal");
                await JS.InvokeVoidAsync("showAlert", "success", "Đã thanh toán");
                await InitializeButtonVisibilityAsync(selectedTableNumber);
                await GetTableColorAsync(selectedTableNumber);
                tableButtonVisibility = await _localStorageService.GetDictionaryAsync<int, ButtonVisibility>("tableButtonVisibility") ?? new Dictionary<int, ButtonVisibility>();
                StateHasChanged();
            }
            else
            {
                await JS.InvokeVoidAsync("showAlert", "error", "Lỗi");
            }

        }

        private async Task SaveRewarPointes(Order _order)
        {
            try
            {
                if (customer != null)
                {
                    int newRewardPoints = (int)(_order.TotalAmount / 100000);

                    var rewardPointe = new RewardPointe()
                    {
                        CustomerId = customer.CustomerId,
                        RewardPoint = newRewardPoints,
                        UpdateDate = DateTime.Now,
                        IsDeleted = false,
                        OrderId = _order.OrderId
                    };

                    var response = await httpClient.PostAsJsonAsync("api/RewardPointe/AddRewardPointe", rewardPointe);
                    if (response.IsSuccessStatusCode)
                    {
                        var createdRewardPoint = await response.Content.ReadFromJsonAsync<RewardPointe>();
                        if(createdRewardPoint != null)
                        {
                            customer.TotalRewardPoint += createdRewardPoint.RewardPoint;
                            Console.WriteLine(customer.TotalRewardPoint);
                            var res = await httpClient.PutAsJsonAsync($"api/Customer/{customer.CustomerId}", customer);
                            if (!res.IsSuccessStatusCode) { await JS.InvokeVoidAsync("showAlert", "error", "Lỗi", "Điểm chưa được thêm"); return; }
                        }
                        else
                        {
                            await JS.InvokeVoidAsync("showAlert", "error", "Lỗi", "Điểm chưa được thêm");
                            return;
                        }
                    }
                    else
                    {
                        await JS.InvokeVoidAsync("showAlert", "error", "Lỗi", "Điểm chưa được thêm");
                        return;
                    }
                }
                else
                {
                    await JS.InvokeVoidAsync("showAlert", "error", "Lỗi", "Khách hàng không tìm thấy");
                    return;
                }
            }
            catch (Exception ex)
            {
                await JS.InvokeVoidAsync("showAlert", "error", "Lỗi", ex.Message);
                return;
            }
        }

        private async Task<Customer> GetCustomerById(int customerId)
        {
            var customer = await httpClient.GetFromJsonAsync<Customer>($"api/Customer/{customerId}");
            if(customer == null) { return null; }
            return customer;
        }

        private async Task<Order> GetOrderInvoice(int orderId)
        {
            var response = await httpClient.PostAsJsonAsync("api/Order/GetOrderInvoice", orderId);

            if (response.IsSuccessStatusCode)
            {
                var responseContent = await response.Content.ReadFromJsonAsync<Order>();

                if (responseContent != null)
                {
                    return responseContent;
                }
                return null;
            }
            else
            {
                order = null;
                return null;
            }

        }

        private void CancelOrder()
        {
            
        }
        private async Task ConfirmRequestAsync(int RequestId)
        {
            var requestToConfirm = requests.FirstOrDefault(r => r.RequestId == RequestId);
            if (requestToConfirm is not null)
            {
                requestToConfirm.IsCompleted = true;
                requestToConfirm.Time = DateTime.Now;

                await _localStorageService.SetListAsync("requests", requests);
                StateHasChanged();
            }
        }
        //Visibility&Color
        private async Task GetTableColorAsync(int tableNumber)
        {
            try
            {
                if (!tableColorsCache.ContainsKey(tableNumber))
                {
                    tableColorsCache[tableNumber] = new ColorTable();
                }

                var color = tableColorsCache[tableNumber];
                string previousColor = color.Color;

                if (!string.IsNullOrEmpty(messagePay) && numtables.Contains(tableNumber) && IsUsing)
                {
                    color.Color = "#FFD700";
                }
                else if (IsUsing && !string.IsNullOrEmpty(numberTable))
                {
                    color.Color = "#ADD8E6";
                }
                else if (IsUsing)
                {
                    color.Color = "#FFA500";
                }
                else if (!string.IsNullOrEmpty(numberTable))
                {
                    color.Color = "#ADD8E6";
                }
                else
                {
                    color.Color = "#32CD32";
                }

                if (previousColor != color.Color)
                {
                    await _localStorageService.SetDictionaryAsync("tableColorsCache", tableColorsCache);
                    StateHasChanged();
                }
            }
            catch
            {
                await JS.InvokeVoidAsync("showAlert", "error", "Lỗi", "Vui lòng liên hệ Admin");
            }
        }
        private async Task InitializeButtonVisibilityAsync(int tableNumber)
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
            await _localStorageService.SetDictionaryAsync("tableButtonVisibility", tableButtonVisibility);
            StateHasChanged();
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
                await JS.InvokeVoidAsync("showAlert", "error", "Lỗi",ex.Message);
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

        [JSInvokable("BusyCall")]
        public void BusyCall()
        {

        }

        [JSInvokable("LstCall")]
        public void LstCall(List<CallInfo> numberCall)
        {
            if(numberCall.Count > 0)
            {
                callInfos = numberCall.ToList();
            }
            StateHasChanged();
            JS.InvokeVoidAsync("showAler", "success", callInfos.Count);
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

        private async Task<Employee> GetEmployeeByAccountId(int accountId)
        {
            var response = await httpClient.PostAsJsonAsync("api/Employee/GetEmployeeByAccountId", accountId);

            if (response.IsSuccessStatusCode)
            {
                var responseContent = await response.Content.ReadFromJsonAsync<Employee>();

                if (responseContent != null)
                {
                    return responseContent;
                }
                return null;
            }
            else
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                await JS.InvokeVoidAsync("showAlert", "error", "Lỗi", $"Lỗi khi gọi API: {response.StatusCode} - Nội dung: {errorContent}");
            }

            return null;
        }
        private async Task<string> CheckTypeAccountId()
        {
            var token = await _localStorageService.GetItemAsync("authToken");
            if (!string.IsNullOrEmpty(token))
            {
                var handler = new JwtSecurityTokenHandler();

                if (handler.ReadToken(token) is JwtSecurityToken jwtToken)
                {
                    var accountTypeClaim = jwtToken.Claims.FirstOrDefault(c => c.Type.Equals("AccountId"));
                    return accountTypeClaim?.Value;
                }
                else
                {
                    await JS.InvokeVoidAsync("showAlert", "error", "Lỗi", "Vui lòng liên hệ Admin");
                }
            }
            return null;
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
    public class ColorTable
    {
        public string Color { get; set; }
    }
    public class Voiecall
    {
        public string NumberTable { get; set; }
        public DateTime Time { get; set; }
        public static List<Voiecall> VoiecallList { get; set;}
    }
    public class CallInfo
    {
        public string FromNumber { get; set; }
        public string Time { get; set; }
    }
}
