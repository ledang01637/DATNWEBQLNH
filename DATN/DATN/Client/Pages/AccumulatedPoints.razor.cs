using DATN.Shared;
using Microsoft.JSInterop;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Json;
using System.Threading.Tasks;

namespace DATN.Client.Pages
{
    public partial class AccumulatedPoints
    {
        private List<Customer> customers = new List<Customer>();
        private Customer customer = new Customer();
        private CustomerVoucher customerVoucher = new CustomerVoucher();
        private List<Voucher> vouchers = new List<Voucher>();
        private int rewardPoint = 0;
        protected override async Task OnInitializedAsync()
        {
            await Load();
        }
        private async Task Load()
        {
            customers = await httpClient.GetFromJsonAsync<List<Customer>>("api/Customer/GetCustomer");
            vouchers = await httpClient.GetFromJsonAsync<List<Voucher>>("api/Voucher/GetVoucher");
            if (customers != null)
            {
                var accountId = await _localStorageService.GetItemAsync("AccountId");
                customer = customers.FirstOrDefault(c => c.AccountId == int.Parse(accountId));
                rewardPoint = customer.TotalRewardPoint;
                StateHasChanged();
            }
            else
            {
                Console.Write("Customer is null");
            }
            
        }
        private async Task RedeemPoints(int point, int voucherId)
        {
            customer.TotalRewardPoint -= point;
            rewardPoint = customer.TotalRewardPoint;
            var response = await httpClient.PutAsJsonAsync($"api/Customer/{customer.CustomerId}", customer);
            if (response.IsSuccessStatusCode)
            {
                customerVoucher = new CustomerVoucher()
                {
                    CustomerId = customer.CustomerId,
                    VoucherId = voucherId,
                    IsUsed = false,
                    Status = "Chưa sử dụng",
                    RedeemDate = DateTime.Now,
                    ExpirationDate = DateTime.Now 
                };
                await httpClient.PostAsJsonAsync("api/CustomerVoucher/AddCustomerVoucher", customerVoucher);
                await JS.InvokeVoidAsync("showAlert", "success", "Đổi voucher thành công", customerVoucher.VoucherId);
            }
            await Load();
            StateHasChanged();
        }
    }
}
