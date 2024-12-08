using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System;
using System.Net.Http.Json;
using System.Globalization;
using DATN.Shared;
using OfficeOpenXml;
using OfficeOpenXml.Style;

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
            Categories = await httpClient.GetFromJsonAsync<List<DATN.Shared.Category>>("api/Category/GetCategories");

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

        public async Task ExportCustomerAndRevenueData()
        {
            try
            {
                // Thiết lập LicenseContext để tránh lỗi System.Drawing
                ExcelPackage.LicenseContext = LicenseContext.NonCommercial;

                using var package = new ExcelPackage();
                var sheet = package.Workbook.Worksheets.Add("Báo Cáo Thống Kê");

                // Định dạng tiêu đề cho bảng đầu tiên
                sheet.Cells["A1:H1"].Merge = true;
                sheet.Cells["A1"].Value = "BÁO CÁO THỐNG KÊ DOANH THU THEO THÁNG NĂM " + DateTime.Now.Year;
                sheet.Cells["A1"].Style.Font.Size = 14;
                sheet.Cells["A1"].Style.Font.Bold = true;
                sheet.Cells["A1"].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;

                // Headers cho bảng đầu tiên
                var headers = new string[] {
                    "Tháng",
                    "Số lượng khách hàng",
                    "Doanh thu (VND)",
                    "VAT (10%)",
                    "Doanh thu sau thuế",
                    "Chi phí (Điền vào nếu có)",
                    "Lợi nhuận",
                    "Tỷ suất lợi nhuận (%)"
                };

                for (int i = 0; i < headers.Length; i++)
                {
                    sheet.Cells[3, i + 1].Value = headers[i];
                    sheet.Cells[3, i + 1].Style.Font.Bold = true;
                }

                // Dữ liệu cho bảng đầu tiên
                var customerData = ListGuest.GroupBy(x => x.CreateDate.Month)
                    .ToDictionary(g => g.Key, g => g.Count());
                var revenueData = listRevenue.GroupBy(x => x.CreateDate.Month)
                    .ToDictionary(g => g.Key, g => g.Sum(x => x.TotalAmount));

                for (int month = 1; month <= 12; month++)
                {
                    int row = month + 3;
                    var revenue = revenueData.GetValueOrDefault(month, 0);
                    var vat = revenue * 0.1m;
                    var revenueAfterTax = revenue - vat;

                    // Đặt công thức tính lợi nhuận trong ô Excel
                    sheet.Cells[row, 7].Formula = $"IF(F{row}=0, E{row}, E{row}-F{row})"; // IF(Chi phí phát sinh = 0, Lợi nhuận = Doanh thu sau thuế, Lợi nhuận = Doanh thu sau thuế - Chi phí phát sinh)

                    // Các ô khác
                    sheet.Cells[row, 1].Value = $"Tháng {month}";
                    sheet.Cells[row, 2].Value = customerData.GetValueOrDefault(month, 0);
                    sheet.Cells[row, 3].Value = revenue;
                    sheet.Cells[row, 4].Value = vat;
                    sheet.Cells[row, 5].Value = revenueAfterTax;
                    sheet.Cells[row, 6].Value = 0; // Chi phí phát sinh mặc định là 0
                    sheet.Cells[row, 8].Formula = $"IF(C{row}=0, 0, G{row}/C{row}*100)"; // Tỷ suất lợi nhuận (%)
                }


                // Tổng cộng cho bảng đầu tiên
                var totalRow = 16;
                sheet.Cells[totalRow, 1].Value = "Tổng cộng";
                sheet.Cells[totalRow, 2].Formula = $"SUM(B4:B15)";
                sheet.Cells[totalRow, 3].Formula = $"SUM(C4:C15)";
                sheet.Cells[totalRow, 4].Formula = $"SUM(D4:D15)";
                sheet.Cells[totalRow, 5].Formula = $"SUM(E4:E15)";
                sheet.Cells[totalRow, 6].Formula = $"SUM(F4:F15)";
                sheet.Cells[totalRow, 7].Formula = $"SUM(G4:G15)";
                sheet.Cells[totalRow, 8].Formula = $"AVERAGE(H4:H15)";

                // Định dạng dòng tổng cộng
                sheet.Cells[totalRow, 1, totalRow, headers.Length].Style.Font.Bold = true;

                // Định dạng số liệu cho bảng đầu tiên
                var dataRange = sheet.Cells[4, 2, totalRow, headers.Length];
                dataRange.Style.Numberformat.Format = "#,##0";
                sheet.Cells[4, 8, totalRow, 8].Style.Numberformat.Format = "#,##0.00";

                // Tự động điều chỉnh độ rộng cột cho bảng đầu tiên
                sheet.Cells.AutoFitColumns();

                // Thêm viền xung quanh bảng đầu tiên
                var border = sheet.Cells[3, 1, totalRow, headers.Length].Style.Border;
                border.Top.Style = border.Bottom.Style = border.Left.Style = border.Right.Style = ExcelBorderStyle.Thin;

                // -----------------
                // Bảng thứ hai: Doanh thu món ăn phổ biến
                // -----------------
                int startRowForSecondTable = totalRow + 2; // Đặt bảng thứ hai bắt đầu từ dòng kế tiếp sau bảng đầu tiên

                sheet.Cells[startRowForSecondTable, 1, startRowForSecondTable, 3].Merge = true;
                sheet.Cells[startRowForSecondTable, 1].Value = "TOP 5 MÓN BÁN CHẠY NHẤT";
                sheet.Cells[startRowForSecondTable, 1].Style.Font.Size = 14;
                sheet.Cells[startRowForSecondTable, 1].Style.Font.Bold = true;
                sheet.Cells[startRowForSecondTable, 1].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;

                // Headers cho bảng thứ hai
                var dishHeaders = new string[] {
                    "Món ăn",
                    "Số lượng bán",
                    "Doanh thu (VND)"
                };

                for (int i = 0; i < dishHeaders.Length; i++)
                {
                    sheet.Cells[startRowForSecondTable + 1, i + 1].Value = dishHeaders[i];
                    sheet.Cells[startRowForSecondTable + 1, i + 1].Style.Font.Bold = true;
                }

                // Dữ liệu cho bảng thứ hai (giả sử bạn đã có dữ liệu cho món ăn phổ biến)
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
                              TotalAmount = orderItemGroup.Quantity * product.Price
                          })
                    .OrderByDescending(dish => dish.Quantity)
                    .Take(5)
                    .ToList();

                for (int i = 0; i < popularDishes.Count; i++)
                {
                    sheet.Cells[startRowForSecondTable + 2 + i, 1].Value = popularDishes[i].ProductName;
                    sheet.Cells[startRowForSecondTable + 2 + i, 2].Value = popularDishes[i].Quantity;
                    sheet.Cells[startRowForSecondTable + 2 + i, 3].Value = popularDishes[i].TotalAmount;
                }

                // Định dạng số liệu cho bảng thứ hai
                var dishDataRange = sheet.Cells[startRowForSecondTable + 2, 2, startRowForSecondTable + 2 + popularDishes.Count - 1, 3];
                dishDataRange.Style.Numberformat.Format = "#,##0";

                // Tự động điều chỉnh độ rộng cột cho bảng thứ hai
                sheet.Cells.AutoFitColumns();

                // Thêm viền xung quanh bảng thứ hai
                var border2 = sheet.Cells[startRowForSecondTable + 1, 1, startRowForSecondTable + 2 + popularDishes.Count - 1, 3].Style.Border;
                border2.Top.Style = border2.Bottom.Style = border2.Left.Style = border2.Right.Style = ExcelBorderStyle.Thin;

                // Lưu file
                var fileBytes = package.GetAsByteArray();
                await JSRuntime.InvokeVoidAsync("saveAsFile", "Bao_Cao_Thong_Ke_Theo_Thang_Nam" + DateTime.Now.Year + ".xlsx", Convert.ToBase64String(fileBytes));
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Lỗi: {ex.Message}");
            }
        }

        private async Task ExportRevenueByDayToExcel()
        {
            try
            {
                // Thiết lập LicenseContext để tránh lỗi System.Drawing
                ExcelPackage.LicenseContext = LicenseContext.NonCommercial;

                using var package = new ExcelPackage();
                var sheet = package.Workbook.Worksheets.Add("Báo Cáo Thống Kê");

                // Xác định ngày đầu và ngày cuối của tuần được chọn
                var startOfWeek = FirstDateOfWeek(selectedDailyYear ?? 2024, selectedWeek ?? 1); // 2024 là giá trị mặc định
                var endOfWeek = startOfWeek.AddDays(6);

                // Định dạng tiêu đề cho báo cáo
                sheet.Cells["A1:H1"].Merge = true;
                string title = $"BÁO CÁO DOANH THU VÀ KHÁCH HÀNG THEO NGÀY TRONG TUẦN ({startOfWeek:dd/MM/yyyy} - {endOfWeek:dd/MM/yyyy})";
                sheet.Cells["A1"].Value = title;
                sheet.Cells["A1"].Style.Font.Size = 14;
                sheet.Cells["A1"].Style.Font.Bold = true;
                sheet.Cells["A1"].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                sheet.Cells["A1"].Style.VerticalAlignment = ExcelVerticalAlignment.Center;

                // Headers cho bảng theo ngày
                var headers = new string[] {
                    "Ngày trong tuần",
                    "Số lượng khách hàng",
                    "Doanh thu (VND)",
                    "VAT (10%)",
                    "Doanh thu sau thuế",
                    "Chi phí (Điền vào nếu có)",
                    "Lợi nhuận",
                    "Tỷ suất lợi nhuận (%)"
                };

                for (int i = 0; i < headers.Length; i++)
                {
                    sheet.Cells[3, i + 1].Value = headers[i];
                    sheet.Cells[3, i + 1].Style.Font.Bold = true;
                    sheet.Cells[3, i + 1].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                    sheet.Cells[3, i + 1].Style.Border.Top.Style = ExcelBorderStyle.Thin;
                    sheet.Cells[3, i + 1].Style.Border.Bottom.Style = ExcelBorderStyle.Thin;
                    sheet.Cells[3, i + 1].Style.Border.Left.Style = ExcelBorderStyle.Thin;
                    sheet.Cells[3, i + 1].Style.Border.Right.Style = ExcelBorderStyle.Thin;
                }

                


                // Dữ liệu theo ngày trong tuần
                var customerDataByDay = ListGuest
                    .Where(x => x.CreateDate >= startOfWeek && x.CreateDate <= endOfWeek)
                    .GroupBy(x => x.CreateDate.DayOfWeek)
                    .ToDictionary(g => g.Key, g => g.Count());

                var revenueDataByDay = listRevenue
                    .Where(x => x.CreateDate >= startOfWeek && x.CreateDate <= endOfWeek)
                    .GroupBy(x => x.CreateDate.DayOfWeek)
                    .ToDictionary(g => g.Key, g => g.Sum(x => x.TotalAmount));

                var daysOfWeek = new[] {
                    DayOfWeek.Monday, DayOfWeek.Tuesday, DayOfWeek.Wednesday, DayOfWeek.Thursday,
                    DayOfWeek.Friday, DayOfWeek.Saturday, DayOfWeek.Sunday
                };

                var vietnameseDays = new[] {
                    "Thứ 2", "Thứ 3", "Thứ 4", "Thứ 5", "Thứ 6", "Thứ 7", "Chủ nhật"
                };

                for (int i = 0; i < daysOfWeek.Length; i++)
                {
                    int row = i + 4; // Dữ liệu bắt đầu từ dòng 4
                    var day = daysOfWeek[i];
                    var revenue = revenueDataByDay.GetValueOrDefault(day, 0);
                    var vat = revenue * 0.1m;
                    var revenueAfterTax = revenue - vat;

                    // Đặt công thức tính lợi nhuận trong ô Excel (tránh lỗi chia cho 0)
                    sheet.Cells[row, 7].Formula = $"IF(F{row}=0, E{row}, E{row}-F{row})"; // Lợi nhuận

                    // Các ô khác
                    sheet.Cells[row, 1].Value = vietnameseDays[i]; // Sửa để lấy tên ngày trong tuần bằng tiếng Việt
                    sheet.Cells[row, 2].Value = customerDataByDay.GetValueOrDefault(day, 0);
                    sheet.Cells[row, 3].Value = revenue;
                    sheet.Cells[row, 4].Value = vat;
                    sheet.Cells[row, 5].Value = revenueAfterTax;
                    sheet.Cells[row, 6].Value = 0; // Chi phí phát sinh mặc định là 0 (sửa nếu có dữ liệu thực tế)
                    sheet.Cells[row, 8].Formula = $"IF(C{row}=0, 0, G{row}/C{row}*100)"; // Tỷ suất lợi nhuận (%)

                }

                // Tổng cộng cho bảng theo ngày
                var totalRow = 11; // Dòng tổng cộng
                sheet.Cells[totalRow, 1].Value = "Tổng cộng";
                sheet.Cells[totalRow, 2].Formula = $"SUM(B4:B10)";
                sheet.Cells[totalRow, 3].Formula = $"SUM(C4:C10)";
                sheet.Cells[totalRow, 4].Formula = $"SUM(D4:D10)";
                sheet.Cells[totalRow, 5].Formula = $"SUM(E4:E10)";
                sheet.Cells[totalRow, 6].Formula = $"SUM(F4:F10)";
                sheet.Cells[totalRow, 7].Formula = $"SUM(G4:G10)";
                sheet.Cells[totalRow, 8].Formula = $"AVERAGE(H4:H10)";

                // Định dạng số liệu
                // Định dạng số liệu cho bảng đầu tiên
                var dataRange = sheet.Cells[4, 2, totalRow, headers.Length];
                dataRange.Style.Numberformat.Format = "#,##0";
                sheet.Cells[4, 8, totalRow, 8].Style.Numberformat.Format = "#,##0.00";

                // Định dạng dòng tổng cộng
                sheet.Cells[totalRow, 1, totalRow, headers.Length].Style.Font.Bold = true;
                sheet.Cells[totalRow, 1, totalRow, headers.Length].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;

                // Thêm viền xung quanh bảng 
                var border = sheet.Cells[3, 1, totalRow, headers.Length].Style.Border;
                border.Top.Style = border.Bottom.Style = border.Left.Style = border.Right.Style = ExcelBorderStyle.Thin;

                // Tự động điều chỉnh độ rộng cột cho bảng
                sheet.Cells.AutoFitColumns();

                // Lưu file Excel vào bộ nhớ
                var fileBytes = package.GetAsByteArray();

                // Lưu file
                await JSRuntime.InvokeVoidAsync("saveAsFile", "Doanh_Thu_Theo_Ngay_" + startOfWeek + "_" + endOfWeek +".xlsx", fileBytes);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Lỗi: {ex.Message}");
            }
        }

        public DateTime FirstDateOfWeek(int year, int weekOfYear)
        {
            var jan1 = new DateTime(year, 1, 1);
            var daysOffset = DayOfWeek.Monday - jan1.DayOfWeek;
            var firstMonday = jan1.AddDays(daysOffset);

            return firstMonday.AddDays((weekOfYear - 1) * 7);
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
