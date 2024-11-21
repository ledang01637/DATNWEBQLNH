
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
    public partial class AdminOrder
    {
        private List<DATN.Shared.Order> listOrder = new List<DATN.Shared.Order>();
        private List<DATN.Shared.OrderItem> listOrderItem = new List<DATN.Shared.OrderItem>();
        private List<DATN.Shared.Product> listProduct = new List<DATN.Shared.Product>();
        private List<DATN.Shared.Customer> listCustomer = new List<DATN.Shared.Customer>();
        private List<DATN.Shared.Order> filter = new List<DATN.Shared.Order>();
        private bool isLoaded = false;
        private string errorMessage;

        protected override async Task OnInitializedAsync()
        {
            await LoadOrders();
            isLoaded = true;
        }

        private async Task LoadOrders()
        {
            try
            {
                listOrder = await httpClient.GetFromJsonAsync<List<DATN.Shared.Order>>("api/Order/GetOrder");
                listProduct = await httpClient.GetFromJsonAsync<List<DATN.Shared.Product>>("api/Product/GetProduct");
                listCustomer = await httpClient.GetFromJsonAsync<List<DATN.Shared.Customer>>("api/Customer/GetCustomer");
                listOrderItem = await httpClient.GetFromJsonAsync<List<DATN.Shared.OrderItem>>("api/OrderItem/GetOrderItem");
                filter = listOrder;
            }
            catch (Exception ex)
            {
                errorMessage = $"Error loading menuitem: {ex.Message}";
            }
        }

        private async Task HideOrder(int orderId)
        {
            try
            {
                var order = listOrder.FirstOrDefault(p => p.OrderId == orderId);
                if (order != null)
                {
                    await httpClient.PutAsJsonAsync($"api/Order/{orderId}", order);
                    await LoadOrders();
                    StateHasChanged();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error hiding menuitem: {ex.Message}");
            }
        }

        private async Task RestoreOrder(int orderId)
        {
            try
            {
                var order = listOrder.FirstOrDefault(p => p.OrderId == orderId);
                if (order != null)
                {
                    await httpClient.PutAsJsonAsync($"api/Order/{orderId}", order);
                    await LoadOrders();
                    StateHasChanged();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error : {ex.Message}");
            }
        }

        private void EditOrder(int orderId)
        {
            Navigation.NavigateTo($"/editorder/{orderId}");
        }

        private void CreateOrder()
        {
            Navigation.NavigateTo($"/createorder");
        }

        //private void Filter(ChangeEventArgs e)
        //{
        //    var searchTerm = e.Value.ToString().ToLower();
        //    filter = string.IsNullOrWhiteSpace(searchTerm)
        //        ? listOrder
        //        : listOrder.Where(p => p.MenuItemId.ToLower().Contains(searchTerm)).ToList();
        //}

    }
}


