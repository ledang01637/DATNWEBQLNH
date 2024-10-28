using DATN.Client.Pages.AdminManager;
using System.Collections.Generic;
using System;
using DATN.Shared;

namespace DATN.Client.Service
{
    public class AppState
    {
        public List<int> NumTables { get; set; } = new();
        public Dictionary<int, CartNote> CartsByTable { get; set; } = new();
        public CartNote CartNote { get; set; } = new();
        public List<RequestCustomer> Requests { get; set; } = new();

        public event Action OnChange;

        public void NotifyStateChanged() => OnChange?.Invoke();
    }
}
