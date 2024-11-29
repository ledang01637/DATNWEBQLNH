using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System;
using System.Net.Http.Json;
using System.Globalization;
using DATN.Shared;
using Microsoft.VisualBasic;
using System.Net.Http;

namespace DATN.Client.Pages.AdminManager
{
    public partial class AdminStatistic
    {
        private int? selectedMonthlyYear;
        private int? selectedDailyYear;
        private int? selectedWeek;
		private int? selectedCustomerYear;
		private int selectedFoodMonth = DateTime.Now.Month;  // Mặc định là tháng hiện tại
		private List<int> availableCustomerYears = new List<int>();
		private List<int> availableYears = new() { };
        private List<int> availableWeeks = new List<int>();
        private Dictionary<int, List<int>> availableWeeksByYear = new Dictionary<int, List<int>>(); // Lưu danh sách tuần theo từng năm
        private List<PopularDish> ListPopularDishes = new List<PopularDish>();
        private List<DATN.Shared.Order> Orders = new List<DATN.Shared.Order>();
        private List<DATN.Shared.OrderItem> OrderItems = new List<DATN.Shared.OrderItem>();
        private List<DATN.Shared.Product> Products = new List<DATN.Shared.Product>();
        private List<DATN.Shared.Category> Categories = new List<DATN.Shared.Category>();
        private bool isMockData = true;  // Đổi thành `false` nếu dùng dữ liệu thực tế từ API

        private ElementReference revenueChartRef;
        private ElementReference monthlyRevenueChartRef;
        private ElementReference popularDishesChartRef;
        private ElementReference customerCountChartRef;

        private readonly string[] days = { "Thứ 2", "Thứ 3", "Thứ 4", "Thứ 5", "Thứ 6", "Thứ 7", "Chủ nhật" };

        protected override void OnInitialized()
        {
            // Khởi tạo danh sách năm
            availableYears = listRevenue.Select(order => order.CreateDate.Year).Distinct().ToList();
        }

	    protected override async Task OnAfterRenderAsync(bool firstRender)
	    {
		    if (firstRender)
		    {
			    try
			    {
				    await LoadMonthlyRevenueChart();

				    // Kiểm tra ListMonthlyRevenue
				    if (ListMonthlyRevenue == null || !ListMonthlyRevenue.Any())
				    {
					    Console.WriteLine(ListMonthlyRevenue);
					    availableYears = new List<int> { DateTime.Now.Year };
					    selectedDailyYear = DateTime.Now.Year;
					    selectedMonthlyYear = DateTime.Now.Year;
					    availableWeeksByYear = new Dictionary<int, List<int>>
			            {
				            {
					            DateTime.Now.Year,
					            new List<int>
					            {
						            CultureInfo.InvariantCulture.Calendar.GetWeekOfYear(
							            DateTime.Now,
							            CalendarWeekRule.FirstFourDayWeek,
							            DayOfWeek.Monday
						            )
					            }
				            }
			            };
					    selectedWeek = availableWeeksByYear[DateTime.Now.Year].First();
					    await LoadCharts();
					    return;
				    }

				    // Lấy các năm có trong hóa đơn và sắp xếp giảm dần
				    availableYears = ListMonthlyRevenue
					    .Select(o => o.CreateDate.Year)
					    .Distinct()
					    .OrderByDescending(y => y)
					    .ToList();

				    if (!availableYears.Any())
				    {
					    availableYears.Add(DateTime.Now.Year);
				    }

				    // Cập nhật năm được chọn
				    var currentYear = DateTime.Now.Year;
				    selectedDailyYear = availableYears.Contains(currentYear)
					    ? currentYear
					    : selectedDailyYear = availableYears.Any() ? availableYears.FirstOrDefault() : DateTime.Now.Year;
				    selectedMonthlyYear = selectedDailyYear;

				    // Khởi tạo từ điển tuần theo năm
				    availableWeeksByYear ??= new Dictionary<int, List<int>>();
				    availableWeeksByYear = ListMonthlyRevenue
					    .GroupBy(order => order.CreateDate.Year)
					    .ToDictionary(
						    g => g.Key,
						    g => g.Select(order =>
							    CultureInfo.InvariantCulture.Calendar.GetWeekOfYear(
								    order.CreateDate,
								    CalendarWeekRule.FirstFourDayWeek,
								    DayOfWeek.Monday
							    )
						    ).Distinct().OrderBy(w => w).ToList()
					    );

				    // Cập nhật tuần mặc định
				    if (selectedDailyYear.HasValue)
				    {
					    if (!availableWeeksByYear.ContainsKey(selectedDailyYear.Value) ||
						    !availableWeeksByYear[selectedDailyYear.Value].Any())
					    {
						    availableWeeksByYear[selectedDailyYear.Value] = new List<int>
				            {
					            CultureInfo.InvariantCulture.Calendar.GetWeekOfYear(
						            DateTime.Now,
						            CalendarWeekRule.FirstFourDayWeek,
						            DayOfWeek.Monday
					            )
				            };
					    }

					    selectedWeek = availableWeeksByYear[selectedDailyYear.Value].LastOrDefault();
				    }

				    // Load tất cả biểu đồ
				    await LoadCharts();

				    StateHasChanged();
			    }
			    catch (Exception ex)
			    {
				    Console.WriteLine($"Lỗi trong OnAfterRenderAsync: {ex.Message}");
			    }
		    }
	    }




	    private async Task LoadCharts()
        {
            await Task.WhenAll(
                LoadRevenueChart(),
                LoadMonthlyRevenueChart(),
                LoadPopularDishesChart(),
                LoadCustomerCountChart()
            );
            StateHasChanged();
        }
        private async Task OnYearSelected(ChangeEventArgs e)
        {
            selectedMonthlyYear = int.Parse(e.Value.ToString());
            await LoadMonthlyRevenueChart();
        }
        private async Task UpdateSelectedYear(int year)
        {
            selectedDailyYear = year;

            // Cập nhật danh sách tuần cho năm đã chọn
            if (availableWeeksByYear.ContainsKey(year))
            {
                availableWeeks = availableWeeksByYear[year];
                selectedWeek = availableWeeks.FirstOrDefault(); // Đặt tuần mặc định là tuần đầu tiên của năm đã chọn
            }
            // Load lại biểu đồ
            await LoadRevenueChart();
        }
        private async Task UpdateSelectedWeek(int week)
        {
            selectedWeek = week;
            await LoadRevenueChart(); // Tải lại biểu đồ với dữ liệu tuần mới
        }

        private List<MonthModel> months = Enumerable.Range(1, 12).Select(m => new MonthModel
        {
            MonthNumber = m,
            MonthName = CultureInfo.CurrentCulture.DateTimeFormat.GetMonthName(m)
        })
        .ToList();

        //private async Task OnMonthSelected(ChangeEventArgs e)
        //{
        //    selectedFoodMonth = int.Parse(e.Value.ToString());
        //    await LoadPopularDishesChart();
        //}
		private async Task OnCustomerYearSelected(ChangeEventArgs e)
		{
			selectedCustomerYear = int.Parse(e.Value.ToString());
			await LoadCustomerCountChart();
		}

		private List<DATN.Shared.Order> listRevenue = new List<DATN.Shared.Order>();
        private async Task LoadRevenueChart()
        {
            // Lấy danh sách đơn hàng từ API
            listRevenue = await httpClient.GetFromJsonAsync<List<DATN.Shared.Order>>("api/Order/GetOrder");

            // Sử dụng dữ liệu cứng
            var orders = listRevenue;

            // Kiểm tra nếu `selectedYear` và `selectedWeek` có giá trị
            if (!selectedDailyYear.HasValue || !selectedWeek.HasValue)
                return;

            var selectedWeekData = listRevenue
                .Where(order =>
                    order.CreateDate.Year == selectedDailyYear.Value &&
                    CultureInfo.InvariantCulture.Calendar.GetWeekOfYear(order.CreateDate, CalendarWeekRule.FirstFourDayWeek, DayOfWeek.Monday) == selectedWeek)
                .GroupBy(order => order.CreateDate.Date)
                .Select(g => new
                {
                    Date = g.Key,
                    TotalRevenue = g.Sum(order => order.TotalAmount)
                })
                .OrderBy(g => g.Date)
                .ToList();

            // Chuẩn bị dữ liệu cho biểu đồ
            var days = selectedWeekData.Select(r => r.Date.ToString("dd/MM")).ToArray();
            var data = selectedWeekData.Select(r => r.TotalRevenue).ToArray();

            var chartData = new
            {
                labels = days,
                datasets = new[]
                {
                        new
                        {
                            label = "Doanh thu",
                            data = data,
                            borderColor = "#4caf50", // Màu xanh lá cây đậm
                            backgroundColor = "rgba(76, 175, 80, 0.5)", // Màu xanh lá cây với độ trong suốt 50%
                            borderWidth = 1,
                            borderRadius = 5,
                            borderSkipped = false
                        }
                    }
            };
            var options = new
            {
                responsive = true,
                maintainAspectRatio = false
            };
            await JSRuntime.InvokeVoidAsync("renderChart", revenueChartRef, "bar", chartData, options);
        }


        private List<DATN.Shared.Order> ListMonthlyRevenue = new List<DATN.Shared.Order>();
        private async Task LoadMonthlyRevenueChart()
        {
            ListMonthlyRevenue = await httpClient.GetFromJsonAsync<List<DATN.Shared.Order>>("api/Order/GetOrder");
            var orders = ListMonthlyRevenue;
            if (orders == null || !orders.Any())
            {
                Console.WriteLine("No orders available.");
                return;
            }

            var monthlyRevenue = orders
                .Where(order => order.CreateDate.Year == selectedMonthlyYear)
                .GroupBy(order => order.CreateDate.Month)
                .Select(g => new { Month = g.Key, TotalRevenue = g.Sum(order => order.TotalAmount) })
                .OrderBy(g => g.Month)
                .ToList();

            var months = monthlyRevenue.Select(r => $"{r.Month}/{selectedMonthlyYear}").ToArray();
            var data = monthlyRevenue.Select(r => r.TotalRevenue).ToArray();
            var chartData = new
            {
                labels = months,
                datasets = new[]
                {
                        new
                        {
                            label = "Doanh thu tháng",
                            data = data,
                            borderColor = "#1b7ced", // Màu xanh dương tương đương Utils.CHART_COLORS.blue
                            backgroundColor = "rgba(27, 124, 237, 0.5)", // Màu xanh dương với độ trong suốt 50%
                            borderWidth = 1,
                            borderRadius = 5,
                            borderSkipped = false
                        }
                    }
            };
            var chartOptions = new
            {
                responsive = true,
                maintainAspectRatio = false,
                scales = new
                {
                    y = new
                    {
                        beginAtZero = true,
                        ticks = new
                        {
                            max = 10 // Giới hạn trục Y, bạn có thể điều chỉnh giá trị này tùy theo yêu cầu
                        }
                    }
                }
            };
            await JSRuntime.InvokeVoidAsync("renderChart", monthlyRevenueChartRef, "bar", chartData, chartOptions);
        }

        private async Task LoadPopularDishesChart()
        {
            Orders = await httpClient.GetFromJsonAsync<List<Order>>("api/Order/GetOrder");
            OrderItems = await httpClient.GetFromJsonAsync<List<OrderItem>>("api/OrderItem/GetOrderItem");
            Products = await httpClient.GetFromJsonAsync<List<Product>>("api/Product/GetProduct");
            Categories = await httpClient.GetFromJsonAsync<List<Category>>("api/Category/GetCategories");

            Console.WriteLine("Orders"+ Orders);
            if (Orders != null && OrderItems != null && Products != null && Categories != null)
            {
                var popularDishes = OrderItems
                    .GroupBy(item => item.ProductId)
                    .Select(group => new
                    {
                        ProductId = group.Key,
                        Quantity = group.Sum(item => item.Quantity)
                    })
                    .Join(Products,
                          orderItemGroup => orderItemGroup.ProductId,
                          product => product.ProductId,
                          (orderItemGroup, product) => new
                          {
                              ProductId = product.ProductId,
                              ProductName = product.ProductName,
                              Quantity = orderItemGroup.Quantity,
                              CategoryId = product.CategoryId 
                          })
                    .OrderByDescending(dish => dish.Quantity)
                    .Take(5)
                    .Select(dish => new PopularDish
                    {
                        ProductId = dish.ProductId,
                        ProductName = dish.ProductName,
                        Quantity = dish.Quantity
                    })
                    .ToList();

                // Prepare data for the chart
                var chartData = new
                {
                    labels = popularDishes.Select(d => d.ProductName).ToArray(),
                    datasets = new[]
                    {
                        new
                        {
                            data = popularDishes.Select(d => d.Quantity).ToArray(),
                            backgroundColor = new[] { "#FF6384", "#36A2EB", "#FFCE56", "#4BC0C0", "#9966FF" }
                        }
                    }
                };

                var options = new
                {
                    responsive = true,
                    maintainAspectRatio = false,
                    plugins = new
                    {
                        legend = new
                        {
                            display = true,
                            position = "right"
                        },
                        title = new
                        {
                            display = true,
                            text = "Top 5 món ăn được đặt nhiều nhất"
                        }
                    }
                };

                // Render the chart with JavaScript interop
                await JSRuntime.InvokeVoidAsync("renderChart", popularDishesChartRef, "doughnut", chartData, options);
            }
        }

	    private List<DATN.Shared.Order> ListGuest = new List<DATN.Shared.Order>();
	    private async Task LoadCustomerCountChart()
        {
			try
			{
				// Gọi API để lấy danh sách hóa đơn
				ListGuest = await httpClient.GetFromJsonAsync<List<Order>>("api/Order/GetOrder");
                var orders = ListGuest;
				// Lấy danh sách năm từ dữ liệu
				availableCustomerYears = orders
					.Select(order => order.CreateDate.Year)
					.Distinct()
					.OrderByDescending(year => year)
					.ToList();

				// Nếu chưa chọn năm, đặt mặc định là năm hiện tại hoặc năm đầu tiên
				if (!selectedCustomerYear.HasValue)
				{
					var currentYear = DateTime.Now.Year;
					selectedCustomerYear = availableCustomerYears.Contains(currentYear) ? currentYear : availableCustomerYears.FirstOrDefault();
				}

				// Lọc dữ liệu theo năm được chọn
				var ordersByYear = orders
					.Where(order => order.CreateDate.Year == selectedCustomerYear)
					.GroupBy(order => order.CreateDate.Month)
					.Select(group => new
					{
						Month = group.Key,
						Count = group.Count() // Đếm số lượng đơn hàng theo tháng
					})
					.ToDictionary(g => g.Month, g => g.Count);

				// Tạo dữ liệu biểu đồ
				var labels = Enumerable.Range(1, 12)
					.Select(m => CultureInfo.CurrentCulture.DateTimeFormat.GetAbbreviatedMonthName(m))
					.ToArray();
				var data = Enumerable.Range(1, 12)
					.Select(m => ordersByYear.ContainsKey(m) ? ordersByYear[m] : 0)
					.ToArray();

				var chartData = new
				{
					labels,
					datasets = new[]
					{
				new
				{
					label = "Số lượng khách hàng theo tháng",
					data,
					borderColor = "#ff9800",
					fill = false
				}
			}
				};

				var options = new
				{
					responsive = true,
					maintainAspectRatio = false
				};

				// Render biểu đồ
				await JSRuntime.InvokeVoidAsync("renderChart", customerCountChartRef, "line", chartData, options);
			}
			catch (Exception ex)
			{
				Console.WriteLine($"Lỗi khi tải dữ liệu khách hàng: {ex.Message}");
			}
		}

        // Class để lưu thông tin tháng
        public class MonthModel
        {
            public int MonthNumber { get; set; }
            public string MonthName { get; set; }
        }
        public class PopularDish
        {
            public int ProductId { get; set; }
            public string ProductName { get; set; }
            public int Quantity { get; set; }
        }
    }
}
