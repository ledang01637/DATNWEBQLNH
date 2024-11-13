using DATN.Shared;
using Microsoft.JSInterop;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Net.Http.Json;
using System.Threading.Tasks;

namespace DATN.Client.Pages
{
    public partial class AccumulatedPoints
    {
        private Customer customer = new();
        private CustomerVoucher customerVoucher = new();
        private List<Voucher> vouchers = new();
        private DateTime expriationDate;

        protected override async Task OnInitializedAsync()
        {
            await LoadAsync();
        }

        private async Task LoadAsync()
        {
            vouchers = await FetchVouchersAsync();

            string accountId = await GetAccountIdAsync();
            if (string.IsNullOrEmpty(accountId))
            {
                await JS.InvokeVoidAsync("showAlert", "warning", "Thông báo", "Vui lòng đăng nhập lại");
                return;
            }

            customer = await FetchCustomerByAccountAsync(int.Parse(accountId)) ?? new Customer();
        }

        private async Task<List<Voucher>> FetchVouchersAsync()
        {
            try
            {
                return await httpClient.GetFromJsonAsync<List<Voucher>>("api/Voucher/GetVoucher") ?? new List<Voucher>();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error fetching vouchers: {ex.Message}");
                return new List<Voucher>();
            }
        }

        private async Task<Customer> FetchCustomerByAccountAsync(int accountId)
        {
            try
            {
                var response = await httpClient.PostAsJsonAsync("api/Customer/GetCustomerByAccountId", accountId);
                return response.IsSuccessStatusCode ? await response.Content.ReadFromJsonAsync<Customer>() : null;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error fetching customer: {ex.Message}");
                return null;
            }
        }

        private async Task<CustomerVoucher> FetchCustomerVoucherAsync(int voucherId, int customerId)
        {
            try
            {
                var url = $"api/CustomerVoucher/GetCustomerVoucherExist?voucherId={voucherId}&customerId={customerId}";
                var customerVoucher = await httpClient.GetFromJsonAsync<CustomerVoucher>(url);

                if (customerVoucher != null)
                {
                    return customerVoucher;
                }
                else
                {
                    Console.WriteLine("CustomerVoucher not found.");
                    return null;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error fetching customer voucher: {ex.Message}");
                return null;
            }
        }

        private async Task RedeemPointsAsync(int points, int voucherId, DateTime _expriationDate)
        {
            expriationDate = _expriationDate;

            if (customer.TotalRewardPoint < points)
            {
                await JS.InvokeVoidAsync("showAlert", "warning", "Thông báo", "Điểm không đủ để đổi voucher.");
                return;
            }

            customerVoucher = await FetchCustomerVoucherAsync(voucherId, customer.CustomerId);

            if (customerVoucher != null && customerVoucher.CustomerVoucherId > 0)
            {
                await JS.InvokeVoidAsync("showAlert", "warning", "Thông báo", "Bạn đã có voucher này rồi");
                return;
            }

            customer.TotalRewardPoint -= points;

            if (await UpdateCustomerPointsAsync(customer))
            {
                await CreateCustomerVoucherAsync(voucherId);
                await JS.InvokeVoidAsync("showAlert", "success", "Thông báo", "Đổi thành công");
            }

            await LoadAsync();
            StateHasChanged();
        }

        private async Task<bool> UpdateCustomerPointsAsync(Customer customer)
        {
            var response = await httpClient.PutAsJsonAsync($"api/Customer/{customer.CustomerId}", customer);
            return response.IsSuccessStatusCode;
        }

        private async Task CreateCustomerVoucherAsync(int voucherId)
        {
            customerVoucher = new CustomerVoucher
            {
                CustomerId = customer.CustomerId,
                VoucherId = voucherId,
                IsUsed = false,
                Status = "Chưa sử dụng",
                RedeemDate = DateTime.Now,
                ExpirationDate = expriationDate,
                IsDeleted = false,
            };

            await httpClient.PostAsJsonAsync("api/CustomerVoucher/AddCustomerVoucher", customerVoucher);
        }

        private async Task<string> GetAccountIdAsync()
        {
            var token = await _localStorageService.GetItemAsync("authToken");
            if (string.IsNullOrEmpty(token)) return null;

            var handler = new JwtSecurityTokenHandler();
            var jwtToken = handler.ReadToken(token) as JwtSecurityToken;
            return jwtToken?.Claims.FirstOrDefault(c => c.Type == "AccountId")?.Value;
        }

    }
}
