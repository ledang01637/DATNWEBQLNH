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

namespace DATN.Client.Pages.AdminManager
{
    public partial class AdminStatistic
    {
        private int? selectedMonthlyYear;
        private int? selectedDailyYear;
        private int? selectedWeek;
        private List<int> availableYears = new() { };
        private List<int> availableWeeks = new List<int>();
        private Dictionary<int, List<int>> availableWeeksByYear = new Dictionary<int, List<int>>(); // Lưu danh sách tuần theo từng năm

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
                // Lấy các năm có trong hóa đơn
                availableYears = ListMonthlyRevenue
                    .Select(o => o.CreateDate.Year)
                    .Distinct()
                    .OrderByDescending(y => y)
                    .ToList();

                // Cập nhật năm được chọn là năm hiện tại hoặc năm lớn nhất có trong dữ liệu
                var currentYear = DateTime.Now.Year;
                selectedDailyYear = availableYears.Contains(currentYear) ? currentYear : availableYears.First();
                selectedMonthlyYear = selectedDailyYear;

                // Khởi tạo từ điển tuần theo năm
                availableWeeksByYear = listRevenue
                    .GroupBy(order => order.CreateDate.Year)
                    .ToDictionary(
                        g => g.Key,
                        g => g.Select(order =>
                            CultureInfo.InvariantCulture.Calendar.GetWeekOfYear(
                                order.CreateDate,
                                CalendarWeekRule.FirstFourDayWeek,
                                DayOfWeek.Monday
                            )
                        ).Distinct().ToList()
                    );

                // Khởi tạo giá trị mặc định cho biểu đồ theo ngày
                selectedDailyYear = availableYears.FirstOrDefault();
                if (selectedDailyYear.HasValue && availableWeeksByYear.ContainsKey(selectedDailyYear.Value))
                {
                    selectedWeek = availableWeeksByYear[selectedDailyYear.Value].FirstOrDefault();
                }

                // Khởi tạo giá trị mặc định cho biểu đồ theo tháng
                selectedMonthlyYear = availableYears.FirstOrDefault();

                // Load lại tất cả biểu đồ
                await LoadCharts();
                StateHasChanged();
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


        private List<DATN.Shared.Order> listRevenue = new List<DATN.Shared.Order>()
        {
            new DATN.Shared.Order { OrderId = 1, CreateDate = new DateTime(2023, 1, 15), TotalAmount = 1200 },
            new DATN.Shared.Order { OrderId = 15, CreateDate = new DateTime(2023, 1, 16), TotalAmount = 2000 },
            new DATN.Shared.Order { OrderId = 16, CreateDate = new DateTime(2023, 1, 16), TotalAmount = 2000 },
            new DATN.Shared.Order { OrderId = 17, CreateDate = new DateTime(2023, 1, 17), TotalAmount = 5400 },
            new DATN.Shared.Order { OrderId = 18, CreateDate = new DateTime(2023, 1, 18), TotalAmount = 9100 },
            new DATN.Shared.Order { OrderId = 2, CreateDate = new DateTime(2023, 2, 10), TotalAmount = 1500 },
            new DATN.Shared.Order { OrderId = 3, CreateDate = new DateTime(2023, 3, 20), TotalAmount = 1800 },
            new DATN.Shared.Order { OrderId = 4, CreateDate = new DateTime(2023, 4, 5), TotalAmount = 2200 },
            new DATN.Shared.Order { OrderId = 5, CreateDate = new DateTime(2023, 5, 18), TotalAmount = 2000 },
            new DATN.Shared.Order { OrderId = 6, CreateDate = new DateTime(2023, 6, 25), TotalAmount = 2400 },
            new DATN.Shared.Order { OrderId = 7, CreateDate = new DateTime(2023, 7, 10), TotalAmount = 2600 },
            new DATN.Shared.Order { OrderId = 8, CreateDate = new DateTime(2023, 8, 10), TotalAmount = 2600 },
            new DATN.Shared.Order { OrderId = 9, CreateDate = new DateTime(2023, 9, 10), TotalAmount = 2600 },
            new DATN.Shared.Order { OrderId = 10, CreateDate = new DateTime(2023, 10, 10), TotalAmount = 2600 },
            new DATN.Shared.Order { OrderId = 11, CreateDate = new DateTime(2023, 11, 10), TotalAmount = 2600 },
            new DATN.Shared.Order { OrderId = 12, CreateDate = new DateTime(2024, 1, 10), TotalAmount = 2600 },
            new DATN.Shared.Order { OrderId = 13, CreateDate = new DateTime(2024, 2, 10), TotalAmount = 2600 },
            new DATN.Shared.Order { OrderId = 14, CreateDate = new DateTime(2025, 12, 10), TotalAmount = 2600 }
        };
        private async Task LoadRevenueChart()
        {
            // Lấy danh sách đơn hàng từ API
            //var orders = await httpClient.GetFromJsonAsync<List<DATN.Shared.Order>>("api/Order/GetOrder");

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

        
        private List<DATN.Shared.Order> ListMonthlyRevenue = new List<DATN.Shared.Order>(){
                new DATN.Shared.Order { OrderId = 1, CreateDate = new DateTime(2023, 1, 15), TotalAmount = 1200 },
                new DATN.Shared.Order { OrderId = 2, CreateDate = new DateTime(2023, 2, 10), TotalAmount = 1500 },
                new DATN.Shared.Order { OrderId = 3, CreateDate = new DateTime(2023, 3, 20), TotalAmount = 1800 },
                new DATN.Shared.Order { OrderId = 4, CreateDate = new DateTime(2023, 4, 5), TotalAmount = 2200 },
                new DATN.Shared.Order { OrderId = 5, CreateDate = new DateTime(2023, 5, 18), TotalAmount = 2000 },
                new DATN.Shared.Order { OrderId = 6, CreateDate = new DateTime(2023, 6, 25), TotalAmount = 2400 },
                new DATN.Shared.Order { OrderId = 7, CreateDate = new DateTime(2023, 7, 10), TotalAmount = 2600 },
                new DATN.Shared.Order { OrderId = 8, CreateDate = new DateTime(2023, 8, 10), TotalAmount = 2600 },
                new DATN.Shared.Order { OrderId = 9, CreateDate = new DateTime(2023, 9, 10), TotalAmount = 2600 },
                new DATN.Shared.Order { OrderId = 10, CreateDate = new DateTime(2023, 10, 10), TotalAmount = 2600 },
                new DATN.Shared.Order { OrderId = 11, CreateDate = new DateTime(2023, 11, 10), TotalAmount = 2600 },
                new DATN.Shared.Order { OrderId = 12, CreateDate = new DateTime(2024, 1, 10), TotalAmount = 2600 },
                new DATN.Shared.Order { OrderId = 13, CreateDate = new DateTime(2024, 2, 10), TotalAmount = 2600 },
                new DATN.Shared.Order { OrderId = 14, CreateDate = new DateTime(2025, 12, 10), TotalAmount = 2600 }
            };
        private async Task LoadMonthlyRevenueChart()
        {
            //var orders = await httpClient.GetFromJsonAsync<List<DATN.Shared.Order>>("api/Order/GetOrder");
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
                        beginAtZero = true
                    }
                }
            };
            await JSRuntime.InvokeVoidAsync("renderChart", monthlyRevenueChartRef, "bar", chartData, chartOptions);
        }


        private async Task LoadPopularDishesChart()
        {
            var data = new[]
            {
                new { name = "Món A", value = 400 },
                new { name = "Món B", value = 300 },
                new { name = "Món C", value = 200 },
                new { name = "Món D", value = 100 },
                new { name = "Món E", value = 100 }
            };

            var chartData = new
            {
                labels = data.Select(d => d.name).ToArray(),
                datasets = new[]
                {
                    new
                    {
                        data = data.Select(d => d.value).ToArray(),
                        backgroundColor = new[] { "#FF6384", "#36A2EB", "#FFCE56", "#4BC0C0", "#9966FF" }
                    }
                }
            };
            var options = new
            {
                responsive = true,
                maintainAspectRatio = false
            };
            await JSRuntime.InvokeVoidAsync("renderChart", popularDishesChartRef, "doughnut", chartData, options);
        }

        private async Task LoadCustomerCountChart()
        {
            var data = new[] { 50, 100, 150, 80, 200, 300, 400 };
            var chartData = new
            {
                labels = days,
                datasets = new[]
                {
                new
                {
                    label = "Số lượng khách hàng",
                    data = data,
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
            await JSRuntime.InvokeVoidAsync("renderChart", customerCountChartRef, "line", chartData, options);
        }

        public class MonthlyRevenue
        {
            public int Month { get; set; }
            public decimal Revenue { get; set; }
        }
    }
}
