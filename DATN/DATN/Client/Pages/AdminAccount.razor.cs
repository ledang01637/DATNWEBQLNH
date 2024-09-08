﻿
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using System;
using System.Net.Http.Json;
using DATN.Shared;
using Microsoft.AspNetCore.Components;
using System.Linq;
using Microsoft.EntityFrameworkCore.Metadata.Internal;

namespace DATN.Client.Pages
{
    public partial class AdminAccount
    {
        private List<DATN.Shared.Account> list = new List<DATN.Shared.Account>();
        private List<DATN.Shared.Account> filter = new List<DATN.Shared.Account>();
        private bool isLoaded = false;
        private string errorMessage;

        protected override async Task OnInitializedAsync()
        {
            await Load();
            isLoaded = true;
        }

        private async Task Load()
        {
            try
            {
                list = await httpClient.GetFromJsonAsync<List<DATN.Shared.Account>>("api/Account/GetAccount");
                filter = list;
            }
            catch (Exception ex)
            {
                errorMessage = $"Error loading categories: {ex.Message}";
            }
        }

        private async Task DeleteAccount(int accountId)
        {
            try
            {
                var account = list.FirstOrDefault(p => p.AccountId == accountId);
                if (account != null)
                {
                    await httpClient.DeleteAsync($"api/Account/{accountId}");
                    await Load();
                    StateHasChanged();
                }
            }
            catch (Exception ex)
            {
                errorMessage = $"Error deleting account: {ex.Message}";
            }
        }

        private void EditAccount(int accountId)
        {
            Navigation.NavigateTo($"/editaccount/{accountId}");
        }

        private void CreateAccount()
        {
            Navigation.NavigateTo($"/createaccount");
        }

        private void FilterAccount(ChangeEventArgs e)
        {
            var searchTerm = e.Value.ToString().ToLower();
            filter = string.IsNullOrWhiteSpace(searchTerm)
                ? list
                : list.Where(p => p.AccountType.ToLower().Contains(searchTerm)).ToList();
        }

    }
}
