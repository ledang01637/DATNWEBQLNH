using Microsoft.JSInterop;
using System;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using DATN.Shared;
using static DATN.Client.Pages.Login;
using System.Collections.Generic;
using System.Net.Http.Json;

namespace DATN.Client.Pages
{
    public partial class Register
    {
        private Account account = new Account();
        private List<Account> accounts = new List<Account>();

        protected override async Task OnInitializedAsync()
        {
            await LoadAll();
            StateHasChanged();
        }

        private async Task LoadAll()
        {
            try
            {
                accounts = await httpClient.GetFromJsonAsync<List<Account>>("api/Account/GetAccount");
            }
            catch(Exception ex)
            {
                Console.Write(ex.ToString());
                var query = $"[C#] fix error: {ex.Message}";
                await JS.InvokeVoidAsync("openChatGPT", query);
            }
        }
        private async Task HandleValidSubmitAsync()
        {

            try
            {

                var exitsUsername = accounts.FirstOrDefault(x => x.Email.Equals(account.Email));
                if (exitsUsername != null)
                {

                    await JS.InvokeVoidAsync("showAlert", "error","Username đã tồn tại","");
                    return;
                }
                using (SHA1 sha1 = SHA1.Create())
                {
                    byte[] passwordBytes = Encoding.UTF8.GetBytes(account.Password);
                    byte[] hashedBytes = sha1.ComputeHash(passwordBytes);
                    account.Password = Convert.ToBase64String(hashedBytes);
                }
                account.CreateDate = DateTime.Now;
                account.UpdateDate = DateTime.Now;
                account.IsActive = true;
                account.Email = account.Email.ToLower();

                var response = await httpClient.PostAsJsonAsync("api/Account/AddAccount", account);
                if (response.IsSuccessStatusCode)
                {
                    var creatRoleAccount = await response.Content.ReadFromJsonAsync<Account>();
                    if(creatRoleAccount  != null)
                    {
                        RoleAccount roleAccount = new()
                        {
                            IsDeleted = false,
                            AccountId = creatRoleAccount.AccountId,
                            RoleId = 3
                        };
                        var responseRoleAccount = await httpClient.PostAsJsonAsync("api/RoleAccount/AddRoleAccount", roleAccount);

                        if(responseRoleAccount != null)
                        {
                            await JS.InvokeVoidAsync("showAlert", "success","Đăng ký thành công","");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.Write(ex.ToString());
                var query = $"[C#] fix error: {ex.Message}";
                await JS.InvokeVoidAsync("openChatGPT", query);

            }
        }
    }
}
