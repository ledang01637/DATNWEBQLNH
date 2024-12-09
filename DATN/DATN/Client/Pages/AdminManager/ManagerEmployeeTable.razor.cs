using System.IdentityModel.Tokens.Jwt;
using System.Threading.Tasks;
using System;
using Microsoft.JSInterop;
using System.Net.Http.Json;
using System.Linq;
using DATN.Shared;
using System.Collections.Generic;
using Microsoft.AspNetCore.SignalR.Client;
using System.Timers;
using DATN.Client.Service;

namespace DATN.Client.Pages.AdminManager
{
    public partial class ManagerEmployeeTable
    {
        private List<Table> tables = new();
        private List<Floor> floors = new();
        private List<Product> products = new();
        private List<Order> orders = new();
        private List<CallInfo> callInfos = new();
        private List<Reservation> reservations = new();
        private List<Reservation> reservationsLst = new();
        private List<Reservation> reservationsProcess = new();
        private Reservation reservationModel = new();
        private Reservation getReservation = new();
        private Order order = new();
        private Table getTable = new();
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
        private decimal Amount = 0;
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
        private bool isEdit = false;
        private int selectedTableId ;
        private bool isCheckBookTable = false;
        private DateTime selectedDate;
        private DateTime selectedTime;
        private Timer _timer;
        private Dictionary<int, string> timeLeftText = new();
        private int updateCounter = 0;
        private int _orderId;
        private string availableUntil;
        private bool IsProcess = false;
        private readonly string urlBookTable = "/employee-book-table";


        protected override async Task OnInitializedAsync()
        {
            try
            {
                IsProcess = true;
                var uri = Navigation.ToAbsoluteUri(Navigation.Uri);
                var query = Microsoft.AspNetCore.WebUtilities.QueryHelpers.ParseQuery(uri.Query);

                if (query.ContainsKey("isCheckBookTable") && !string.IsNullOrEmpty(query["isCheckBookTable"]))
                {
                    isCheckBookTable = bool.Parse(query["isCheckBookTable"]);
                }
                else
                {
                    isCheckBookTable = false;
                }

                hubConnection = new HubConnectionBuilder()
                    .WithUrl(Navigation.ToAbsoluteUri("/ProcessHub"))
                    .Build();

                await SetupHubEvents();
                await hubConnection.StartAsync();

                string username = await _localStorageService.GetItemAsync("userName");

                var response = await httpClient.PostAsJsonAsync("api/Voice/post-message", username);

                var content = response.Content.ReadAsStringAsync();

                if (!response.IsSuccessStatusCode)
                {
                    Console.WriteLine(content.Result);
                    await JS.InvokeVoidAsync("showAlert", "error", "Lỗi", "Không thể thêm người nhận cuộc gọi");
                }

                await GetLocalStorageAsync();
                await LoadAll();
                await SetupTimer();
            }
            catch
            {
                await JS.InvokeVoidAsync("showAlert", "error", "Lỗi", "Lỗi không xác định");
            }
            finally
            {
                IsProcess = false;
            }
            
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

            hubConnection.On<string, List<CartDTO>, string, int>("UpdateTable", async (_numTable, carts, note,orderId) =>
            {
                numberTable = _numTable;
                _orderId = orderId;
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
                await GetTableColorAsync(numberTablePay);
                orderIdPay = _orderId;
                order = await GetOrderInvoice(orderIdPay) ?? new Order();
                if(_customerId > 0)
                {
                    customer = await GetCustomerById(_customerId) ?? new Customer();
                }
            });

            return Task.CompletedTask;
        }

        //InitLoad
        private async Task LoadAll()
        {
            IsProcess = true;
            try
            {
                var loadTablesTask = httpClient.GetFromJsonAsync<List<Table>>("api/Table/GetTable");
                var loadFloorsTask = httpClient.GetFromJsonAsync<List<Floor>>("api/Floor/GetFloor");
                var loadProductsTask = httpClient.GetFromJsonAsync<List<Product>>("api/Product/GetProduct");
                var loadOrdersTask = httpClient.GetFromJsonAsync<List<Order>>("api/Order/GetOrderLstInclude");
                var loadReservationsTask = httpClient.GetFromJsonAsync<List<Reservation>>("api/Reservation/GetReservation");
                var loadReservationsIncludeTask = httpClient.GetFromJsonAsync<List<Reservation>>("api/Reservation/GetReservationInclude");


                await Task.WhenAll(loadTablesTask, loadFloorsTask, loadProductsTask, loadOrdersTask,  loadReservationsIncludeTask, loadReservationsTask);

                tables = loadTablesTask.Result?.Where(a => !a.IsDeleted).ToList() ?? new List<Table>();
                floors = loadFloorsTask.Result?.Where(a => !a.IsDeleted).ToList() ?? new List<Floor>();
                products = loadProductsTask.Result?.Where(a => !a.IsDeleted).ToList() ?? new List<Product>();
                orders = loadOrdersTask.Result?.Where(a => !a.IsDeleted).ToList() ?? new List<Order>();
                reservationsLst = loadReservationsIncludeTask.Result?.ToList() ?? new List<Reservation>(); ;
                reservations = loadReservationsTask.Result?.Where(a => !a.IsDeleted && a.ReservationStatus.Equals("Đang xử lý")).ToList() ?? new List<Reservation>();
                reservationsProcess = loadReservationsTask.Result?.Where(a => !a.IsDeleted && a.ReservationStatus.Equals("Đặt bàn thành công")).ToList() ?? new List<Reservation>();

                if (tables == null || floors == null || products == null || orders == null)
                {
                    await JS.InvokeVoidAsync("showAlert", "error", "Lỗi", "Không thể tải dữ liệu từ server. Vui lòng thử lại!");
                    return;
                }


                // Kiểm tra tài khoản nhân viên
                var accountId = await CheckTypeAccountId();
                if (accountId != null)
                {
                    employee = await GetEmployeeByAccountId(int.Parse(accountId));
                    if (employee == null || employee.EmployeeId <= 0)
                    {
                        await JS.InvokeVoidAsync("showAlert", "warning", "Thông báo", "Vui lòng đăng nhập bằng tài khoản nhân viên hoặc thêm nhân viên mới");
                        await Task.Delay(1000);
                        Navigation.NavigateTo("/login-admin");
                        return;
                    }
                }
            }
            catch (Exception ex)
            {
                await HandleError(ex);
            }finally
            {
                IsProcess = false;
            }
        }


        #region ProcessOrder

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
            order = await GetOrderInvoice(_orderId);
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
            IsProcess = true;
            try
            {
                await JS.InvokeVoidAsync("closeModal", "tableModal");
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

                await JS.InvokeVoidAsync("showAlert", "success", "Đã gửi đầu bếp");
                await InitializeButtonVisibilityAsync(selectedTableNumber);
                tableButtonVisibility = await _localStorageService.GetDictionaryAsync<int, ButtonVisibility>("tableButtonVisibility") ?? new Dictionary<int, ButtonVisibility>();
                StateHasChanged();
            }
            catch
            {
                await JS.InvokeVoidAsync("showAlert", "error", "Lỗi","Vui lòng gọi Admin");
            }
            finally
            {
                IsProcess = false;
            }

        }

        private async Task RemoveFromCartAsync(CartDTO product)
        {
            if (cartsByTable.TryGetValue(selectedTableNumber, out var existingCartNote))
            {
                var existingCart = existingCartNote.CartDTOs.FirstOrDefault(c => c.ProductId == product.ProductId);
                if (existingCart != null)
                {
                    existingCartNote.CartDTOs.Remove(existingCart);

                    var oi = order.OrderItems.FirstOrDefault(p => p.ProductId == existingCart.ProductId);

                    if (oi != null)
                    {
                        oi.IsDeleted = true;
                        await UpdateQuantityOrderItem(oi);
                        order = await GetOrderInvoice(order.OrderId);
                    }
                }
                
            }
            await _localStorageService.SetAsync("_cartNote", existingCartNote);
            _cartNote = await _localStorageService.GetAsync<CartNote>("_cartNote");
            await _localStorageService.SetDictionaryAsync("cartsByTable", cartsByTable);
            StateHasChanged();
        }
        private async Task DecreaseQuantity(int productId)
        {
            await ModifyQuantity(productId, -1);
        }
        private async Task ModifyQuantity(int productId, int change)
        {
            if (cartsByTable.TryGetValue(selectedTableNumber, out var existingCartNote))
            {
                var existingCart = existingCartNote.CartDTOs.FirstOrDefault(c => c.ProductId == productId);

                if (existingCart == null)
                {
                    await JS.InvokeVoidAsync("showAlert", "warning", "Vui lòng thêm món ăn");
                    return;
                }

                existingCart.Quantity += change;

                if (existingCart.Quantity <= 0)
                {
                  await RemoveFromCartAsync(existingCart);
                }

                var oi = order.OrderItems.FirstOrDefault(p => p.ProductId == existingCart.ProductId);

                if (oi != null)
                {
                    oi.Quantity = existingCart.Quantity;

                    if(oi.Quantity <=0)
                    {
                        oi.IsDeleted = true;
                    }
                    await UpdateQuantityOrderItem(oi);
                    order = await GetOrderInvoice(order.OrderId);
                }
            }

            await _localStorageService.SetAsync("_cartNote", existingCartNote);
            await _localStorageService.SetDictionaryAsync("cartsByTable", cartsByTable);
            StateHasChanged();
        }

        private async Task UpdateQuantityOrderItem(OrderItem orderItem)
        {
            var response = await httpClient.PutAsJsonAsync($"api/OrderItem/{orderItem.OrderItemId}", orderItem);
            if(!response.IsSuccessStatusCode) { await JS.InvokeVoidAsync("showAlert", "Lỗi", "Không thể cập nhật đơn hàng"); return; }
        }

        private async void CalculatorAmount()
        {
            getTable = await httpClient.GetFromJsonAsync<Table>($"api/Table/GetTableByNumber?numberTable={selectedTableNumber}");
            if (getTable == null || getTable.TableId <= 0)
            {
                await JS.InvokeVoidAsync("showAlert", "warning", "Thông báo", "Không tìm thấy bàn"); return;
            }

            if (getTable.Status == "inusebooktable")
            {

                getReservation = await httpClient.GetFromJsonAsync<Reservation>($"api/Reservation/GetReservationByTimeTableId?tableId={getTable.TableId}");

                if (getReservation == null || getReservation.ReservationId <= 0)
                {
                    await JS.InvokeVoidAsync("showAlert", "warning", "Thông báo", "Không tìm thấy đơn đặt bàn"); return;
                }
                getReservation.ReservationStatus = "Hoàn tất";
                getReservation.UpdatedDate = DateTime.Now;

                var resReser = await httpClient.PutAsJsonAsync($"api/Reservation/{getReservation.ReservationId}", getReservation);

                if (!resReser.IsSuccessStatusCode) { await JS.InvokeVoidAsync("showAlert", "warning", "Thông báo", "Không thể cập nhật đơn đặt bàn"); return; }
            }
            TotalAmount = order.TotalAmount;
            order.TableId = getTable.TableId;
        }

        private async void ProcessPayment()
        {
            IsProcess = true;
            try
            {
                await JS.InvokeVoidAsync("closeModal", "invoiceModal");
                if (string.IsNullOrEmpty(messagePay))
                {
                    await JS.InvokeVoidAsync("showAlert", "warning", "Thông báo", "Khách hàng chưa yêu cầu thanh toán");
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
                    order.TotalAmount = TotalAmount;
                    order.EmployeeId = employee.EmployeeId;
                    order.Note = _cartNote.Note;


                    if (customer != null && customer.CustomerId > 0 && customer.Email != "no account")
                    {
                        order.CustomerId = customer.CustomerId;
                        await SaveRewarPointes(order);
                    }

                    var response = await httpClient.PutAsJsonAsync($"api/Order/{order.OrderId}", order);

                    if (!response.IsSuccessStatusCode) { await JS.InvokeVoidAsync("showAlert", "error", "Lỗi", "Vui lòng liên hệ Admin update order"); return; }

                    getTable.Status = "empty";

                    var res = await httpClient.PutAsJsonAsync($"api/Table/{getTable.TableId}", getTable);

                    if (!res.IsSuccessStatusCode) { await JS.InvokeVoidAsync("showAlert", "warning", "Thông báo", "Không thể cập nhật bàn"); return; }


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
                    return;
                }
                else
                {
                    await JS.InvokeVoidAsync("showAlert", "error", "Lỗi", "Không tìn thấy hóa đơn");
                }
            }
            catch(Exception ex)
            {
                var query = $"[C#] fix error bằng tiếng việt: {ex.Message}";
                await JS.InvokeVoidAsync("openChatGPT", query);
            }
            finally
            {
                IsProcess = false;
            }
            

        }

        private async Task SaveRewarPointes(Order _order)
        {
            IsProcess = true;
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
            catch
            {
                await JS.InvokeVoidAsync("showAlert", "error", "Lỗi", "Không lưu điểm được");
                return;
            }
            finally
            {
                IsProcess = false;
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

        private async void CancelOrder()
        {
            await JS.InvokeVoidAsync("showAlert","success","Thông báo","Hủy thành công");
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
            var query = $"[C#] fix error bằng tiếng Việt: {ex.Message}";
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
        #endregion

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
            IsProcess = true;
            try
            {
                var response = await httpClient.PostAsJsonAsync("api/JwtTokenValidator/ValidateToken", token);
                if (response.IsSuccessStatusCode)
                {
                    var handler = new JwtSecurityTokenHandler();

                    if (handler.ReadToken(token) is not JwtSecurityToken jsonToken)
                    {
                        await JS.InvokeVoidAsync("showAlert", "error", "Lỗi","Vui lòng thử đăng nhập lại nếu không được thì liên hệ Admin");
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
                            await JS.InvokeVoidAsync("showAlert", "error", "Lỗi","Vui lòng đăng nhập lại");
                            Navigation.NavigateTo("/login-admin");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                var query = $"[C#] fix error bằng tiếng việt: {ex.Message}";
                await JS.InvokeVoidAsync("openChatGPT", query);
                await JS.InvokeVoidAsync("showAlert", "error", "Lỗi","Không thể setup cuộc gọi");

            }finally
            {
                IsProcess = false;
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

        #region BookTable

        private Task SetupTimer()
        {
            foreach (var table in tables)
            {
                var reservation = reservationsProcess.FirstOrDefault(r => r.TableId == table.TableId);

                if (reservation != null && reservation.ReservationTime > DateTime.Now)
                {
                    timeLeftText[table.TableNumber] = FormatTimeLeft(reservation.ReservationTime - DateTime.Now);
                }
                else
                {
                    timeLeftText[table.TableNumber] = null;
                }
            }

            _timer = new Timer(1000);
            _timer.Elapsed += UpdateCountdownAsync;
            _timer.AutoReset = true;
            _timer.Start();

            return Task.CompletedTask;
        }

        private async void UpdateCountdownAsync(object sender, ElapsedEventArgs e)
        {
            foreach (var table in tables)
            {
                var reservation = reservationsProcess.FirstOrDefault(r => r.TableId == table.TableId);

                if (reservation != null)
                {
                    var timeLeft = reservation.ReservationTime - DateTime.Now;

                    if (timeLeft.TotalSeconds <= 0 && table.Status != "inusebooktable")
                    {
                        table.Status = "inusebooktable";

                        reservation.ReservationStatus = "Đã nhận bàn";

                        await ReceivedTableAsync(reservation, table);
                    }
                    else if (timeLeft.TotalHours <= 2.5 && table.Status != "availableuntil")
                    {
                        table.Status = "availableuntil";

                        availableUntil = "availableuntil";

                        _ = UpdateTableStatusAsync(table);

                        timeLeftText[table.TableNumber] = FormatTimeLeft(timeLeft);
                    }
                    else
                    {
                        timeLeftText[table.TableNumber] = FormatTimeLeft(timeLeft);
                    }
                }
                else
                {
                    timeLeftText[table.TableNumber] = null;
                }
            }

            if (++updateCounter % 2 == 0)
            {
                UpdateReservations();
            }

            _ = InvokeAsync(StateHasChanged);
        }

        private string FormatTimeLeft(TimeSpan timeLeft)
        {
            if (timeLeft.TotalHours <= 2.5)
            {
                return $"Khách đến sau: {timeLeft.Hours}h {timeLeft.Minutes}m {timeLeft.Seconds}s";
            }

            return $"Bàn sẽ khóa sau: {timeLeft.Hours}h {timeLeft.Minutes}m {timeLeft.Seconds}s";
        }


        private async Task ProcessBookTable(int reservationId, bool isCancel)
        {
            IsProcess = true;
            try
            {
                reservationModel = await httpClient.GetFromJsonAsync<Reservation>($"api/Reservation/{reservationId}");

                if (reservationModel != null)
                {
                    if (isCancel)
                    {
                        reservationModel.ReservationStatus = "Đã hủy";
                        reservationModel.UpdatedDate = DateTime.Now;
                        await JS.InvokeVoidAsync("showModal", "ConfirmCancelModal");
                    }
                    else
                    {
                        selectedDate = reservationModel.ReservationTime.Date;
                        selectedTime = reservationModel.ReservationTime;
                        await JS.InvokeVoidAsync("showModal", "bookTableModal");
                    }
                }
                else
                {
                    await JS.InvokeVoidAsync("showAlert", "warning","Thông báo","Không tìm thấy đơn đặt bàn");
                }
            }
            catch
            {
                await JS.InvokeVoidAsync("showAlert", "error", "Lỗi", "Không thể xử lý bàn");
            }
            finally
            {
                IsProcess = false;
            }
        }

        //Update số lượng thông báo
        private async void UpdateReservations()
        {
            var loadReservationsTask = httpClient.GetFromJsonAsync<List<Reservation>>("api/Reservation/GetReservation");

            await Task.WhenAll(loadReservationsTask);

            reservations = loadReservationsTask.Result?.Where(a => !a.IsDeleted && a.ReservationStatus.Equals("Đang xử lý")).ToList() ?? new List<Reservation>();
            reservationsProcess = loadReservationsTask.Result?.Where(a => !a.IsDeleted && a.ReservationStatus.Equals("Đặt bàn thành công")).ToList() ?? new List<Reservation>();
            StateHasChanged(); 
        }

        private void EditInforCustomer()
        {
            isEdit = true;
            StateHasChanged();
        }

        private void ConfirmInforCustomer()
        {
            CatulatorDepositPaymentAsync();
            isEdit = false;
            reservationModel.UpdatedDate = DateTime.Now;
        }

        private void ProcessChooseTable(int tableId)
        {
            selectedTableId = tableId;
        }

        private async void OnSubmitForm()
        {
            await CatulatorDepositPaymentAsync();
            if (selectedTableId <= 0)
            {
                await JS.InvokeVoidAsync("showAlert", "warning", "Thông báo", "Vui lòng chọn bàn.");
                return;
            }

            reservationModel.TableId = selectedTableId;
            reservationModel.IsPayment = true;
            reservationModel.UpdatedDate = DateTime.Now;
            reservationModel.ReservationStatus = "Đặt bàn thành công";

            reservationModel.ReservationTime = new DateTime(
                    selectedDate.Year,
                    selectedDate.Month,
                    selectedDate.Day,
                    selectedTime.Hour,
                    selectedTime.Minute,
                    0
                );

            if (reservationModel.ReservationTime < DateTime.Now.AddHours(2))
            {
                await JS.InvokeVoidAsync("showAlert", "warning", "Thông báo", "Thời gian đặt bàn phải ít nhất sau 2 giờ kể từ hiện tại.");
                return;
            }

            var hasConflict = reservationsProcess.Any(r =>
                r.TableId == reservationModel.TableId &&
                !r.IsDeleted &&
                r.ReservationTime.AddMinutes(-150) < reservationModel.ReservationTime && 
                r.ReservationTime.AddMinutes(150) > reservationModel.ReservationTime); 

            if (hasConflict)
            {
                await JS.InvokeVoidAsync("showAlert", "warning", "Thông báo", "Bàn đã được đặt trong khoảng thời gian này. Vui lòng chọn bàn hoặc thời gian khác cách ít nhất 2 giờ 30 phút.");
                return;
            }

            var tableBook = await httpClient.GetFromJsonAsync<Table>($"api/Table/{selectedTableId}");

            if( tableBook != null && tableBook.TableId > 0)
            {
                tableBook.Status = "reserved";
                var updateTableStatus = await httpClient.PutAsJsonAsync($"api/Table/{selectedTableId}", tableBook);

                if(!updateTableStatus.IsSuccessStatusCode) 
                {
                    await JS.InvokeVoidAsync("showAlert", "error", "Lỗi", "Cập nhật trạng thái bàn thất bại.");
                    return;
                }
            }
            else
            {
                await JS.InvokeVoidAsync("showAlert", "warning", "Thông báo", "Không tìm thấy bàn.");
                return;
            }

            var response =  await httpClient.PutAsJsonAsync($"api/Reservation/{reservationModel.ReservationId}", reservationModel);

            if (!response.IsSuccessStatusCode) { await JS.InvokeVoidAsync("showAlert", "error", "Lỗi", "Không thể xác nhận đặt bàn"); return; }

            await JS.InvokeVoidAsync("closeModal", "bookTableModal");
            await JS.InvokeVoidAsync("showAlert", "success", "Thông báo", "Đã xác nhận đặt bàn.");
            StateHasChanged();
        }

        private async void ConfirmCancelBookTable()
        {
            await JS.InvokeVoidAsync("closeModal", "ConfirmCancelModal");
            var response = await httpClient.PutAsJsonAsync($"api/Reservation/{reservationModel.ReservationId}", reservationModel);
            if (!response.IsSuccessStatusCode)
            {
                await JS.InvokeVoidAsync("showAlert", "error", "Lỗi", "Hủy đặt bàn thất bại");
                return;
            }
            await JS.InvokeVoidAsync("showAlert", "success", "Thông báo", "Hủy đặt bàn thành công");
            await Task.Delay(1000);
            Navigation.NavigateTo(Navigation.Uri, true);
        }

        private async Task ReceivedTableAsync(Reservation reservation, Table table)
        {
            reservation.UpdatedDate = DateTime.Now;
            var response = await httpClient.PutAsJsonAsync($"api/Reservation/{reservation.ReservationId}", reservation);

            if (!response.IsSuccessStatusCode)
            {
                await JS.InvokeVoidAsync("showAlert", "error", "Lỗi", "Cập nhật trạng thái thất bại");
                return;
            }
            var resTable = await httpClient.PutAsJsonAsync($"api/Table/{table.TableId}", table);

            if (!resTable.IsSuccessStatusCode)
            {
                await JS.InvokeVoidAsync("showAlert", "error", "Lỗi", "Cập nhật trạng thái thất bại");
                return;
            }
        }

        private async Task UpdateTableStatusAsync(Table table)
        {
            var response = await httpClient.PutAsJsonAsync($"api/Table/{table.TableId}", table);

            if (!response.IsSuccessStatusCode)
            {
                await JS.InvokeVoidAsync("showAlert", "error", "Lỗi", "Cập nhật bàn thất bại");
                return;
            }
            StateHasChanged();
        }

        private Task CatulatorDepositPaymentAsync()
        {
            const decimal adultDeposit = 50000;
            const decimal childDeposit = 0;

            var adults = reservationModel.Adults;
            var children = reservationModel.Children;

            reservationModel.DepositPayment = adults * adultDeposit + children * childDeposit;
            StateHasChanged();
            return Task.CompletedTask;
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
            _timer?.Stop();
            _timer?.Dispose();

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
