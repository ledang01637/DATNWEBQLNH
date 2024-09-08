using Microsoft.JSInterop;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Json;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using DATN.Shared;
using static DATN.Client.Pages.Login;

namespace DATN.Client.Pages
{
    public partial class Register
    {
        private User newUser = new User();
        public List<User> Users = liststaticuser.Users;


        private async Task HandleValidSubmitAsync()
        {
           
            try
            {
                
                // Check if the user already has a registered restaurant
                var exitsPhone = liststaticuser.Users.FirstOrDefault(x => x.PhoneNumber.Equals(newUser.PhoneNumber));
                var exitsEmail = liststaticuser.Users.FirstOrDefault(x => x.Email.Equals(newUser.Email));
                if (exitsPhone != null) // Change from == null to != null
                {

                    await JS.InvokeVoidAsync("showAlert", "InputPhoneExits");
                    return; // Exit the method to prevent further processing
                }
                else if (exitsEmail != null)
                {
                    await JS.InvokeVoidAsync("showAlert", "InputEmailExits");
                    return; // Exit the method to prevent further processing
                }

                // Hash the password using SHA-1
                using (SHA1 sha1 = SHA1.Create())
                {
                    byte[] passwordBytes = Encoding.UTF8.GetBytes(newUser.Password);
                    byte[] hashedBytes = sha1.ComputeHash(passwordBytes);
                    newUser.Password = Convert.ToBase64String(hashedBytes);
                }

                // Simulate adding user to the database
                newUser.Id = liststaticuser.Users.Count + 1;
                newUser.RoleId = 1; // Assuming role_id is 1 for regular users
                newUser.CreatedAt = DateTime.Now;
                newUser.AccountType = "Thông thường"; // Set account type

                liststaticuser.Users.Add(newUser);

                // Clear the form data after successful registration
                newUser = new User();

                // Show success alert
                await JS.InvokeVoidAsync("showAlert", "success");
            }
            catch (Exception ex)
            {
                // Handle any exceptions and show error alert
                await JS.InvokeVoidAsync("showAlert", "error");
               
            }
        }
    }
}
