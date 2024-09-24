
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using System;
using System.Net.Http.Json;
using DATN.Shared;
using Microsoft.AspNetCore.Components;
using System.Linq;
using Microsoft.EntityFrameworkCore.Metadata.Internal;

namespace DATN.Client.Pages.AdminManager
{
    public partial class AdminVoucher
    {
        private List<DATN.Shared.Voucher> listVoucher = new List<DATN.Shared.Voucher>();
        private List<DATN.Shared.CustomerVoucher> listCustomerVoucher = new List<DATN.Shared.CustomerVoucher>();
        private List<DATN.Shared.Voucher> filter = new List<DATN.Shared.Voucher>();
        private bool isLoaded = false;
        private string errorMessage;

        protected override async Task OnInitializedAsync()
        {
            await LoadVouchers();
            isLoaded = true;
        }

        private async Task LoadVouchers()
        {
            try
            {
                listCustomerVoucher = await httpClient.GetFromJsonAsync<List<DATN.Shared.CustomerVoucher>>("api/CustomerVoucher/GetCustomerVoucher");
                listVoucher = await httpClient.GetFromJsonAsync<List<DATN.Shared.Voucher>>("api/Voucher/GetVoucher");
                filter = listVoucher;
            }
            catch (Exception ex)
            {
                errorMessage = $"Error loading unit: {ex.Message}";
            }
        }

        private async Task HideVoucher(int voucherId)
        {
            try
            {
                var voucher = listVoucher.FirstOrDefault(p => p.VoucherId == voucherId);
                if (voucher != null)
                {
                    voucher.IsActive = true;
                    await httpClient.PutAsJsonAsync($"api/Voucher/{voucherId}", voucher);
                    await LoadVouchers();
                    StateHasChanged();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error hiding voucher: {ex.Message}");
            }
        }

        private async Task RestoreVoucher(int voucherId)
        {
            try
            {
                var voucher = listVoucher.FirstOrDefault(p => p.VoucherId == voucherId);
                if (voucher != null)
                {
                    voucher.IsActive = false;
                    await httpClient.PutAsJsonAsync($"api/Voucher/{voucherId}", voucher);
                    await LoadVouchers();
                    StateHasChanged();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Đã xảy ra lỗi khi khôi phục : {ex.Message}");
            }
        }

        private void EditVoucher(int voucherId)
        {
            Navigation.NavigateTo($"/editvoucher/{voucherId}");
        }

        private void CreateVoucher()
        {
            Navigation.NavigateTo($"/createvoucher");
        }

        private void Filter(ChangeEventArgs e)
        {
            var searchTerm = e.Value.ToString().ToLower();
            filter = string.IsNullOrWhiteSpace(searchTerm)
                ? listVoucher
                : listVoucher.Where(p => p.VoucherCode.ToLower().Contains(searchTerm)).ToList();
        }



    }
}


