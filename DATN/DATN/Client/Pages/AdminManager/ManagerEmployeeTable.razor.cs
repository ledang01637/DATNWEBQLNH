﻿using System.IdentityModel.Tokens.Jwt;
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
        private Dictionary<int, PaymentInfo> paymentRequests = new();
        private Dictionary<int, List<Order>> tableOrders = new();
        private Dictionary<int, TableState> tableStates = new();
        private Dictionary<int, ButtonVisibility> tableButtonVisibility = new();
        private Dictionary<int, ColorTable> tableColorsCache = new();
        private Dictionary<int, CartNote> cartsByTable = new();
        private List<int> numtables = new();
        private CartNote _cartNote = new();

        private decimal TotalAmount = 0;
        private decimal Amount = 0;
        private bool IsUsing = false;
        private int selectedTableNumber = 0;
        private static int nextRequestId = 1;
        private string messagePay;
        private string token;
        private string from;
        private string to;
        private bool isEdit = false;
        private int selectedTableId;
        private bool isCheckBookTable = false;
        private DateTime selectedDate;
        private DateTime selectedTime;
        private Timer _timer;
        private readonly Dictionary<int, string> timeLeftText = new();
        private int updateCounter = 0;
        private int _orderId;
        private int numberSeatBookTable = 0;
        private bool IsProcess = false;
        private bool IsProcessOrder = false;
        private bool FirstLoad = false;
        private readonly string urlBookTable = "/employee/employee-book-table";


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
                    .WithAutomaticReconnect()
                    .Build();

                hubConnection.Closed += (error) => {
                    Navigation.NavigateTo(Navigation.Uri, true);
                    return Task.CompletedTask;
                };

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
            catch (Exception ex)
            {
                await JS.InvokeVoidAsync("showAlert", "error", "Lỗi", "Lỗi không xác định");
                await HandleError(ex);
            }
            finally
            {
                IsProcess = false;
            }

        }
        private async Task GetLocalStorageAsync()
        {
            var numTablesTask = _localStorageService.GetListAsync<int>("numtables");
            var requestsTask = _localStorageService.GetListAsync<RequestCustomer>("requests");
            var cartNoteTask = _localStorageService.GetAsync<CartNote>("_cartNote");
            var cartsByTableTask = _localStorageService.GetDictionaryAsync<int, CartNote>("cartsByTable");
            var tableButtonVisibilityTask = _localStorageService.GetDictionaryAsync<int, ButtonVisibility>("tableButtonVisibility");
            var tableColorsCacheTask = _localStorageService.GetDictionaryAsync<int, ColorTable>("tableColorsCache");
            var tableStatesTask = _localStorageService.GetDictionaryAsync<int, TableState>("TableState");
            var tableOrdersTask = _localStorageService.GetDictionaryAsync<int, List<Order>>("tableOrders");

            await Task.WhenAll(numTablesTask, requestsTask, cartNoteTask, cartsByTableTask, tableButtonVisibilityTask, tableColorsCacheTask, tableStatesTask, tableOrdersTask);

            numtables = numTablesTask.Result ?? new List<int>();
            requests = requestsTask.Result ?? new List<RequestCustomer>();
            _cartNote = cartNoteTask.Result ?? new CartNote();
            cartsByTable = cartsByTableTask.Result ?? new Dictionary<int, CartNote>();
            tableButtonVisibility = tableButtonVisibilityTask.Result ?? new Dictionary<int, ButtonVisibility>();
            tableColorsCache = tableColorsCacheTask.Result ?? new Dictionary<int, ColorTable>();
            tableStates = tableStatesTask.Result ?? new Dictionary<int, TableState>();
            tableOrders = tableOrdersTask.Result ?? new Dictionary<int, List<Order>>();
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

            hubConnection.On<string, List<CartDTO>, string, int>("UpdateTable", async (_numTable, carts, note, orderId) =>
            {
                try
                {
                    _orderId = orderId;
                    tableStates = await _localStorageService.GetDictionaryAsync<int, TableState>("tableStates")
                                     ?? new Dictionary<int, TableState>();

                    if (int.TryParse(_numTable, out int tableNumber))
                    {
                        if (!tableStates.ContainsKey(tableNumber))
                        {
                            tableStates[tableNumber] = new TableState();
                        }
                        tableStates[tableNumber].HasPreviousOrder = true;
                        tableStates[tableNumber].IsUsing = false;
                        tableStates[tableNumber].HasPaymentRequest = false;


                        await _localStorageService.SetDictionaryAsync("tableStates", tableStates);
                        await UpdateCartNoteAsync(tableNumber, carts, note);
                        await InitializeButtonVisibilityAsync(tableNumber);
                        await GetTableColorAsync(tableNumber);
                    }
                    await InvokeAsync(StateHasChanged);
                }
                catch (Exception ex)
                {
                    await JS.InvokeVoidAsync("showAlert", "error", "Lỗi", "Lỗi xử lý khi nhận yêu cầu");
                    await HandleError(ex);
                }
            });


            hubConnection.On<string, int, int, int>("ReqPay", async (message, _numberTable, _orderId, _customerId) =>
            {
                try
                {
                    tableStates = await _localStorageService.GetDictionaryAsync<int, TableState>("tableStates")
                                     ?? new Dictionary<int, TableState>();

                    if (!tableStates.ContainsKey(_numberTable))
                    {
                        tableStates[_numberTable] = new TableState();
                    }

                    tableStates[_numberTable].HasPaymentRequest = true;
                    tableStates[_numberTable].IsUsing = false;
                    tableStates[_numberTable].HasPreviousOrder = false;

                    await _localStorageService.SetDictionaryAsync("tableStates", tableStates);

                    var currentOrder = await GetOrderInvoice(_orderId) ?? new Order();

                    if (!tableOrders.ContainsKey(_numberTable))
                    {
                        tableOrders[_numberTable] = new List<Order>();
                    }

                    if (!tableOrders[_numberTable].Any(o => o.OrderId == _orderId))
                    {
                        tableOrders[_numberTable].Add(currentOrder);
                    }

                    await _localStorageService.SetDictionaryAsync("tableOrders", tableOrders);

                    customer = _customerId > 0 ? await GetCustomerById(_customerId) : new Customer();

                    paymentRequests[_numberTable] = new PaymentInfo
                    {
                        Message = message,
                        OrderId = _orderId,
                        Customer = customer
                    };

                    await GetTableColorAsync(_numberTable);
                }
                catch (Exception ex)
                {
                    await JS.InvokeVoidAsync("showAlert", "error", "Lỗi", "Lỗi xử lý khi nhận yêu cầu");
                    await HandleError(ex);
                }
            });

            return Task.CompletedTask;
        }

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


                await Task.WhenAll(loadTablesTask, loadFloorsTask, loadProductsTask, loadOrdersTask, loadReservationsIncludeTask, loadReservationsTask);

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
                        Navigation.NavigateTo("/admin/login-admin");
                        return;
                    }
                }
            }
            catch (Exception ex)
            {
                await HandleError(ex);
            }
            finally
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
        private void ShowModalForTable(int numberTable)
        {
            selectedTableNumber = numberTable;
            _cartNote = cartsByTable.TryGetValue(numberTable, out var existingCartNote)
                ? new CartNote
                {
                    CartDTOs = existingCartNote.CartDTOs.ToList(),
                    PreviousCartDTOs = existingCartNote.PreviousCartDTOs.ToList(),
                    Note = existingCartNote.Note
                }
                : new CartNote
                {
                    CartDTOs = new List<CartDTO>(),
                    PreviousCartDTOs = new List<CartDTO>(),
                    Note = string.Empty
                };
            StateHasChanged();
        }
        private async Task ConfirmOrder()
        {
            IsProcess = true;
            try
            {
                await JS.InvokeVoidAsync("closeModal", "tableModal");

                cartsByTable = await _localStorageService.GetDictionaryAsync<int, CartNote>("cartsByTable") ?? new Dictionary<int, CartNote>();

                if (!cartsByTable.TryGetValue(selectedTableNumber, out var existingCartNote))
                {
                    existingCartNote = new CartNote
                    {
                        CartDTOs = new List<CartDTO>(),
                        PreviousCartDTOs = new List<CartDTO>(),
                        Note = string.Empty
                    };
                }

                _cartNote = existingCartNote;

                if (_cartNote.CartDTOs is null || !_cartNote.CartDTOs.Any())
                {
                    await JS.InvokeVoidAsync("showAlert", "warning", "Không có món mới");
                    return;
                }

                foreach (var newItem in _cartNote.CartDTOs)
                {
                    var existingItem = _cartNote.PreviousCartDTOs.FirstOrDefault(item => item.ProductId == newItem.ProductId);
                    if (existingItem != null)
                    {
                        existingItem.Quantity += newItem.Quantity;
                    }
                    else
                    {
                        _cartNote.PreviousCartDTOs.Add(newItem);
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

                tableStates = await _localStorageService.GetDictionaryAsync<int, TableState>("tableStates")
                 ?? new Dictionary<int, TableState>();

                tableStates[selectedTableNumber].HasPreviousOrder = false;
                tableStates[selectedTableNumber].HasPaymentRequest = false;
                tableStates[selectedTableNumber].IsUsing = true;

                _cartNote.CartDTOs = new List<CartDTO>();
                cartsByTable[selectedTableNumber] = _cartNote;



                await _localStorageService.SetDictionaryAsync("tableStates", tableStates);
                await _localStorageService.SetAsync("_cartNote", existingCartNote);
                await _localStorageService.SetDictionaryAsync("cartsByTable", cartsByTable);
                await GetTableColorAsync(selectedTableNumber);
                await UpdateOrderStatus("processing");
                await JS.InvokeVoidAsync("showAlert", "success", "Thành công ", "Món đã gửi đầu bếp");
                await InitializeButtonVisibilityAsync(selectedTableNumber);

                StateHasChanged();
            }
            catch (Exception ex)
            {
                await JS.InvokeVoidAsync("showAlert", "error", "Lỗi", "Vui lòng gọi Admin");
                await HandleError(ex);
            }
            finally
            {
                IsProcess = false;
            }
        }
        private async Task UpdateOrderStatus(string status)
        {
            var _table = await httpClient.GetFromJsonAsync<Table>($"api/Table/GetTableByNumber?numberTable={selectedTableNumber}");

            if (_table.TableId > 0)
            {
                var _order = await httpClient.GetFromJsonAsync<Order>($"api/Order/GetOrderStatus?tableId={_table.TableId}&status={status}");

                if (_order.OrderId > 0)
                {
                    _order.Status = "unpaid";
                    var response = await httpClient.PutAsJsonAsync($"api/Order/{_order.OrderId}", _order);
                    if (!response.IsSuccessStatusCode) { await JS.InvokeVoidAsync("showAlert", "error", "Lỗi", "Không thể cập nhật hóa đơn"); return; }
                }
                else
                {
                    await JS.InvokeVoidAsync("showAlert", "error", "Lỗi", "Không tìm thấy hóa đơn"); return;
                }

            }
            else
            {
                await JS.InvokeVoidAsync("showAlert", "error", "Lỗi", "Không tìm thấy bàn"); return;
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

                    if (oi.Quantity <= 0)
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
            if (!response.IsSuccessStatusCode) { await JS.InvokeVoidAsync("showAlert", "Lỗi", "Không thể cập nhật đơn hàng"); return; }
        }

        private async void CalculatorAmount()
        {
            IsProcessOrder = true;
            try
            {
                getTable = await httpClient.GetFromJsonAsync<Table>($"api/Table/GetTableByNumber?numberTable={selectedTableNumber}");
                if (getTable == null || getTable.TableId <= 0)
                {
                    await JS.InvokeVoidAsync("showAlert", "warning", "Thông báo", "Không tìm thấy bàn"); return;
                }

                order = await GetOrderForTable();

                if (order == null)
                {
                    await JS.InvokeVoidAsync("showAlert", "error", "Lỗi", "Hóa đơn đã được thanh toán hoặc không tồn tại");
                    return;
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
            catch (Exception ex)
            {
                await HandleError(ex);
                await JS.InvokeVoidAsync("showAlert", "error", "Lỗi", "Lỗi xử lý tính toán hóa đơn");
            }
            finally
            {
                IsProcessOrder = false;
            }

        }
        private async void ProcessPayment()
        {
            IsProcess = true;
            try
            {
                await JS.InvokeVoidAsync("closeModal", "invoiceModal");

                if (!paymentRequests.TryGetValue(selectedTableNumber, out var paymentInfo) || string.IsNullOrEmpty(paymentInfo.Message))
                {
                    await JS.InvokeVoidAsync("showAlert", "warning", "Thông báo", "Khách hàng chưa yêu cầu thanh toán");
                    return;
                }


                cartsByTable = await _localStorageService.GetDictionaryAsync<int, CartNote>("cartsByTable");

                if (cartsByTable is null)
                {
                    await JS.InvokeVoidAsync("showAlert", "error", "Lỗi", "Vui lòng liên hệ Admin!");
                    return;
                }

                if (cartsByTable.TryGetValue(selectedTableNumber, out var existingCartNote))
                {
                    cartsByTable.Remove(selectedTableNumber);
                    IsUsing = false;
                }

                if (order != null && order.OrderId > 0)
                {
                    order.Status = "completed";
                    order.TotalAmount = TotalAmount;
                    order.EmployeeId = employee.EmployeeId;
                    order.Note = _cartNote.Note;

                    if (paymentInfo.Customer != null && paymentInfo.Customer.CustomerId > 0 && paymentInfo.Customer.Email != "no account")
                    {
                        order.CustomerId = paymentInfo.Customer.CustomerId;
                        await SaveRewarPointes(order);
                    }

                    var response = await httpClient.PutAsJsonAsync($"api/Order/{order.OrderId}", order);

                    if (!response.IsSuccessStatusCode)
                    {
                        await JS.InvokeVoidAsync("showAlert", "error", "Lỗi", "Vui lòng liên hệ Admin");
                        return;
                    }

                    getTable.Status = "empty";

                    var res = await httpClient.PutAsJsonAsync($"api/Table/{getTable.TableId}", getTable);

                    if (!res.IsSuccessStatusCode)
                    {
                        await JS.InvokeVoidAsync("showAlert", "warning", "Thông báo", "Không thể cập nhật bàn");
                        return;
                    }

                    tableStates = await _localStorageService.GetDictionaryAsync<int, TableState>("tableStates")
                 ?? new Dictionary<int, TableState>();

                    tableStates[selectedTableNumber].HasPreviousOrder = false;
                    tableStates[selectedTableNumber].IsUsing = false;
                    tableStates[selectedTableNumber].HasPaymentRequest = false;

                    paymentRequests.Remove(selectedTableNumber);
                    TotalAmount = 0;
                    IsUsing = false;



                    tableOrders[selectedTableNumber] = new List<Order>();

                    await _localStorageService.SetDictionaryAsync("tableStates", tableStates);
                    await _localStorageService.SetDictionaryAsync("cartsByTable", cartsByTable);
                    await _localStorageService.SetDictionaryAsync("tableOrders", tableOrders);
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
                    await JS.InvokeVoidAsync("showAlert", "error", "Lỗi", "Không tìm thấy hóa đơn");
                }
            }
            catch (Exception ex)
            {
                await JS.InvokeVoidAsync("showAlert", "error", "Lỗi", "Không thể xử lý hóa đơn");
                await HandleError(ex);
            }
            finally
            {
                IsProcess = false;
            }
        }

        private async Task<Order> GetOrderForTable()
        {
            try
            {
                tableOrders = await _localStorageService.GetDictionaryAsync<int, List<Order>>("tableOrders");

                if (!tableOrders.TryGetValue(selectedTableNumber, out var orders) || orders == null || !orders.Any())
                {
                    await JS.InvokeVoidAsync("showAlert", "error", "Lỗi", "Không tìm thấy hóa đơn cho bàn này");
                    return null;
                }

                var currentOrder = orders.LastOrDefault(order => order.Status != "completed");

                if (currentOrder == null)
                {
                    await JS.InvokeVoidAsync("showAlert", "error", "Lỗi", "Hóa đơn đã được thanh toán hoặc không tồn tại");
                    return null;
                }

                return currentOrder;
            }
            catch (Exception ex)
            {
                await JS.InvokeVoidAsync("showAlert", "error", "Lỗi", $"Lỗi khi lấy hóa đơn: {ex.Message}");
                return null;
            }
        }
        private async Task SaveRewarPointes(Order _order)
        {
            IsProcess = true;
            try
            {
                if (customer != null && customer.CustomerId > 0)
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
                        if (createdRewardPoint != null)
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
            catch (Exception ex)
            {
                await JS.InvokeVoidAsync("showAlert", "error", "Lỗi", "Không lưu điểm được");
                await HandleError(ex);
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
            if (customer == null) { return null; }
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
            await JS.InvokeVoidAsync("showAlert", "success", "Thông báo", "Hủy thành công");
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

                var tableState = await GetTableStateAsync(tableNumber);

                if (tableState.HasPaymentRequest)
                {
                    color.Color = "#FFD700";
                }
                else if (tableState.IsUsing)
                {
                    color.Color = "#FFA500";
                }
                else if (tableState.HasPreviousOrder)
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
            catch (Exception ex)
            {
                await JS.InvokeVoidAsync("showAlert", "error", "Lỗi", "Vui lòng liên hệ Admin");
                await HandleError(ex);
            }
        }
        private async Task<TableState> GetTableStateAsync(int tableNumber)
        {
            tableStates = await _localStorageService.GetDictionaryAsync<int, TableState>("tableStates");

            if (tableStates != null && tableStates.TryGetValue(tableNumber, out var state))
            {
                return state;
            }

            return new TableState
            {
                IsUsing = false,
                HasPaymentRequest = false,
                HasPreviousOrder = false
            };
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
        private static string MergeNotes(string existingNote, string newNote)
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
                await SetupVideo();
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
                        await JS.InvokeVoidAsync("showAlert", "error", "Lỗi", "Vui lòng thử đăng nhập lại nếu không được thì liên hệ Admin");
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
                            await JS.InvokeVoidAsync("showAlert", "error", "Lỗi", "Vui lòng đăng nhập lại");
                            Navigation.NavigateTo("/admin/login-admin");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                await JS.InvokeVoidAsync("showAlert", "error", "Lỗi", "Không thể setup cuộc gọi");
                await HandleError(ex);

            }
            finally
            {
                IsProcess = false;
            }
        }
        private async Task SetupVideo()
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
            if (numberCall.Count > 0)
            {
                callInfos = numberCall.ToList();
            }
            StateHasChanged();
            _ = JS.InvokeVoidAsync("showAler", "success", callInfos.Count);
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

        private static string FormatTimeLeft(TimeSpan timeLeft)
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
                        var _tb = await httpClient.GetFromJsonAsync<Table>($"api/Table/{reservationModel.TableId}");
                        if (_tb != null && _tb.TableId > 0)
                        {
                            _tb.Status = "empty";
                            var response = await httpClient.PutAsJsonAsync($"api/Table/{_tb.TableId}", _tb);
                            if (!response.IsSuccessStatusCode)
                            {
                                await JS.InvokeVoidAsync("showAlert", "error", "Lỗi", "Không thể cập nhật trạng thái bàn");
                                return;
                            }
                        }
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
                    await JS.InvokeVoidAsync("showAlert", "warning", "Thông báo", "Không tìm thấy đơn đặt bàn");
                }
            }
            catch (Exception ex)
            {
                await JS.InvokeVoidAsync("showAlert", "error", "Lỗi", "Không thể xử lý bàn");
                await HandleError(ex);
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

        private async void ConfirmInforCustomer()
        {
            await CatulatorDepositPaymentAsync();
            isEdit = false;
            reservationModel.UpdatedDate = DateTime.Now;
        }

        private void ProcessChooseTable(int tableId)
        {
            selectedTableId = tableId;
        }

        private async Task OnSubmitForm()
        {
            numberSeatBookTable = 0;
            await CatulatorDepositPaymentAsync();

            if (selectedTableId <= 0)
            {
                await JS.InvokeVoidAsync("showAlert", "warning", "Thông báo", "Vui lòng chọn bàn.");
                return;
            }

            var table = await httpClient.GetFromJsonAsync<Table>($"api/Table/{selectedTableId}");

            if (table == null)
            {
                await JS.InvokeVoidAsync("showAlert", "error", "Thông báo", "Không tìm thấy bàn.");
                return;
            }

            var totalSeat = reservationModel.Adults + reservationModel.Children;

            if (totalSeat > table.SeatingCapacity)
            {
                numberSeatBookTable = totalSeat - table.SeatingCapacity;
                await JS.InvokeVoidAsync("closeModal", "bookTableModal");
                await JS.InvokeVoidAsync("showModal", "ConfirmModalSeatTable");
                return;
            }
            else
            {
                await ProcessSeatBookTableAsync(true);
            }
        }

        private async Task ProcessSeatBookTableAsync(bool isAccept)
        {
            if (isAccept)
            {
                reservationModel.TableId = selectedTableId;
                reservationModel.IsPayment = true;
                reservationModel.UpdatedDate = DateTime.Now;
                reservationModel.ReservationStatus = "Đặt bàn thành công";

                reservationModel.ReservationTime = new DateTime(
                    selectedDate.Year, selectedDate.Month, selectedDate.Day,
                    selectedTime.Hour, selectedTime.Minute, 0);

                if (reservationModel.ReservationTime < DateTime.Now.AddHours(2))
                {
                    await JS.InvokeVoidAsync("showAlert", "warning", "Thông báo", "Thời gian đặt bàn phải ít nhất sau 2 giờ kể từ hiện tại.");
                    return;
                }

                if (CheckReservationConflict(reservationModel.TableId, reservationModel.ReservationTime))
                {
                    await JS.InvokeVoidAsync("showAlert", "warning", "Thông báo",
                        "Bàn đã được đặt trong khoảng thời gian này. Vui lòng chọn bàn hoặc thời gian khác cách ít nhất 2 giờ 30 phút.");
                    return;
                }

                var tableBook = await httpClient.GetFromJsonAsync<Table>($"api/Table/{selectedTableId}");

                if (tableBook != null)
                {
                    tableBook.Status = "reserved";
                    var updateTableStatus = await httpClient.PutAsJsonAsync($"api/Table/{selectedTableId}", tableBook);

                    if (!updateTableStatus.IsSuccessStatusCode)
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

                var response = await httpClient.PutAsJsonAsync($"api/Reservation/{reservationModel.ReservationId}", reservationModel);

                if (!response.IsSuccessStatusCode)
                {
                    await JS.InvokeVoidAsync("showAlert", "error", "Lỗi", "Không thể xác nhận đặt bàn.");
                    return;
                }

                await JS.InvokeVoidAsync("closeModal", "ConfirmModalSeatTable");
                await JS.InvokeVoidAsync("closeModal", "bookTableModal");
                await JS.InvokeVoidAsync("showAlert", "success", "Thông báo", "Đã xác nhận đặt bàn.");
                StateHasChanged();
            }
            else
            {
                await JS.InvokeVoidAsync("showModal", "bookTableModal");
                await JS.InvokeVoidAsync("closeModal", "ConfirmModalSeatTable");
            }
        }

        private bool CheckReservationConflict(int tableId, DateTime reservationTime)
        {
            return reservationsProcess.Any(r =>
                r.TableId == tableId &&
                !r.IsDeleted &&
                r.ReservationTime.AddMinutes(-150) < reservationTime &&
                r.ReservationTime.AddMinutes(150) > reservationTime);
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

        private async Task CatulatorDepositPaymentAsync()
        {
            decimal adultDeposit = await GetFee();

            const decimal childDeposit = 0;

            var adults = reservationModel.Adults;
            var children = reservationModel.Children;

            reservationModel.DepositPayment = adults * adultDeposit + children * childDeposit;
            StateHasChanged();
        }

        private async Task<decimal> GetFee()
        {
            try
            {
                string fee = await httpClient.GetStringAsync("api/Fee/get-fee");

                if (string.IsNullOrEmpty(fee))
                {
                    await JS.InvokeVoidAsync("showAlert", "error", "Lỗi", "Không tìm thấy phí đặt cọc");
                    return 0;
                }

                if (decimal.TryParse(fee, out var parsedFee))
                {
                    return parsedFee;
                }
                else
                {
                    await JS.InvokeVoidAsync("showAlert", "error", "Lỗi", "Phí đặt cọc không hợp lệ");
                    return 0;
                }
            }
            catch (Exception ex)
            {
                await JS.InvokeVoidAsync("showAlert", "error", "Lỗi", $"Đã xảy ra lỗi khi lấy phí: {ex.Message}");
                return 0;
            }
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
    #region ClassDTO
    public class CartNote
    {
        public List<CartDTO> CartDTOs { get; set; }
        public List<CartDTO> PreviousCartDTOs { get; set; }
        public string Note { get; set; }
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
        public static List<Voiecall> VoiecallList { get; set; }
    }
    public class CallInfo
    {
        public string FromNumber { get; set; }
        public string Time { get; set; }
    }
    public class PaymentInfo
    {
        public string Message { get; set; }
        public int OrderId { get; set; }
        public Customer Customer { get; set; }
    }
    public class TableState
    {
        public bool IsUsing { get; set; }
        public bool HasPaymentRequest { get; set; }
        public bool HasPreviousOrder { get; set; }
    }
    #endregion
}
