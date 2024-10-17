
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
    public partial class AdminAccount
    {
        private List<DATN.Shared.Account> listAccount = new List<DATN.Shared.Account>();
        private List<DATN.Shared.Customer> listCustomer = new List<DATN.Shared.Customer>();
        private List<DATN.Shared.RoleAccount> listRoleAccount = new List<DATN.Shared.RoleAccount>();
        private List<DATN.Shared.RewardPointe> listRewardPointe = new List<DATN.Shared.RewardPointe>();
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
                listAccount = await httpClient.GetFromJsonAsync<List<DATN.Shared.Account>>("api/Account/GetAccount");
                listRoleAccount = await httpClient.GetFromJsonAsync<List<DATN.Shared.RoleAccount>>("api/RoleAccount/GetRoleAccount");
                listCustomer = await httpClient.GetFromJsonAsync<List<DATN.Shared.Customer>>("api/Customer/GetCustomer");
                listRewardPointe = await httpClient.GetFromJsonAsync<List<DATN.Shared.RewardPointe>>("api/RewardPointe/GetRewardPointe");

                // Lọc tài khoản để không hiển thị Admin
                listAccount = listAccount.Where(p => !p.AccountType.Equals("Admin", StringComparison.OrdinalIgnoreCase)).ToList();

                // Gán filter ban đầu
                filter = listAccount;
            }
            catch (Exception ex)
            {
                errorMessage = $"Error loading categories: {ex.Message}";
            }
        }

        private async Task HideAccount(int accountId)
        {
            try
            {
                var account = listAccount.FirstOrDefault(p => p.AccountId == accountId);
                if (account != null)
                {
                    account.IsActive = false;
                    await httpClient.PutAsJsonAsync($"api/Account/{accountId}", account);
                    await Load();
                    StateHasChanged();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error hiding voucher: {ex.Message}");
            }
        }

        private async Task RestoreAccount(int accountId)
        {
            try
            {
                var account = listAccount.FirstOrDefault(p => p.AccountId == accountId);
                if (account != null)
                {
                    account.IsActive = true;
                    await httpClient.PutAsJsonAsync($"api/Account/{accountId}", account);
                    await Load();
                    StateHasChanged();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Đã xảy ra lỗi khi khôi phục : {ex.Message}");
            }
        }

        private async Task UpdateAccount(int accountId)
        {
            try
            {
                var account = listAccount.FirstOrDefault(p => p.AccountId == accountId);
                var role = listRoleAccount.FirstOrDefault(r => r.AccountId == accountId);

                if (account != null && role != null)
                {
                    account.AccountType = "Employee"; // Cập nhật loại tài khoản
                    role.Roleid = 2; // Cập nhật vai trò thành Employee

                    // Gửi yêu cầu cập nhật tài khoản
                    var accountResponse = await httpClient.PutAsJsonAsync($"api/Account/{accountId}", account);

                    // Kiểm tra nếu tài khoản được cập nhật thành công, tiếp tục cập nhật role
                    if (accountResponse.IsSuccessStatusCode)
                    {
                        // Gửi yêu cầu cập nhật vai trò
                        var roleResponse = await httpClient.PutAsJsonAsync($"api/RoleAccount/{accountId}", role);

                        // Nếu cả hai yêu cầu đều thành công, load lại dữ liệu
                        if (roleResponse.IsSuccessStatusCode)
                        {
                            await Load();
                            StateHasChanged();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Đã xảy ra lỗi khi cập nhật tài khoản: {ex.Message}");
            }
        }

        private async Task DefaultAccount(int accountId)
        {
            try
            {
                var account = listAccount.FirstOrDefault(p => p.AccountId == accountId);
                var role = listRoleAccount.FirstOrDefault(r => r.AccountId == accountId);

                if (account != null && role != null)
                {
                    account.AccountType = "Customer"; // Cập nhật loại tài khoản
                    role.Roleid = 3; // Cập nhật vai trò thành Customer

                    // Gửi yêu cầu cập nhật tài khoản
                    var accountResponse = await httpClient.PutAsJsonAsync($"api/Account/{accountId}", account);

                    // Kiểm tra nếu tài khoản được cập nhật thành công, tiếp tục cập nhật role
                    if (accountResponse.IsSuccessStatusCode)
                    {
                        // Gửi yêu cầu cập nhật vai trò
                        var roleResponse = await httpClient.PutAsJsonAsync($"api/RoleAccount/{accountId}", role);

                        // Nếu cả hai yêu cầu đều thành công, load lại dữ liệu
                        if (roleResponse.IsSuccessStatusCode)
                        {
                            await Load();
                            StateHasChanged();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Đã xảy ra lỗi khi cập nhật tài khoản: {ex.Message}");
            }
        }


        //private async Task UpdateAccount(int accountId)
        //{
        //    try
        //    {
        //        var account = listAccount.FirstOrDefault(p => p.AccountId == accountId);
        //        var role = listRoleAccount.FirstOrDefault(r => r.AccountId == accountId);
        //        if (account != null)
        //        {
        //            account.AccountType = "Employee";
        //            role.Roleid = 2;
        //            await httpClient.PutAsJsonAsync($"api/Account/{accountId}", account);
        //            await Load();
        //            StateHasChanged();
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        Console.WriteLine($"Đã xảy ra lỗi khi khôi phục : {ex.Message}");
        //    }
        //}
        //private async Task DefaultAccount(int accountId)
        //{
        //    try
        //    {
        //        var account = listAccount.FirstOrDefault(p => p.AccountId == accountId);
        //        var role = listRoleAccount.FirstOrDefault(r => r.AccountId == accountId);
        //        if (account != null)
        //        {
        //            account.AccountType = "Customer";
        //            role.Roleid = 3;
        //            await httpClient.PutAsJsonAsync($"api/Account/{accountId}", account);
        //            await Load();
        //            StateHasChanged();
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        Console.WriteLine($"Đã xảy ra lỗi khi khôi phục : {ex.Message}");
        //    }
        //}


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
                ? listAccount
                : listAccount.Where(p => p.AccountType.ToLower().Contains(searchTerm) || p.UserName.ToLower().Contains(searchTerm)).ToList();
        }

    }
}
