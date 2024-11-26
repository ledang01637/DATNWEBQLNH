using Microsoft.AspNetCore.Components.Web;
using System.Collections.Generic;
using System;
using Microsoft.JSInterop;
using System.Threading.Tasks;

namespace DATN.Client.Pages.AdminManager
{
    public partial class DemoChef
    {
        private List<Food> foods = new()
        {
            new Food { Id = 1, Name = "Pizza", Description = "Cheese & tomato topping" },
            new Food { Id = 2, Name = "Burger", Description = "Beef patty with lettuce" },
            new Food { Id = 3, Name = "Sushi", Description = "Rice & fresh fish" },
            new Food { Id = 4, Name = "Pasta", Description = "Pasta with tomato sauce" },
            new Food { Id = 5, Name = "Salad", Description = "Mixed greens with dressing" },
        };

        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            if (firstRender)
            {
                await JS.InvokeVoidAsync("renderFoodList", foods);
            }
        }

        public class Food
        {
            public int Id { get; set; }
            public string Name { get; set; }
            public string Description { get; set; }
        }
    }
}

