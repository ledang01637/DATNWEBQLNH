using System;
using System.Collections.Generic;

namespace DATN.Shared
{
    public class RequestCustomer
    {
        public int RequestId { get; set; }
        public int TableNumbe { get; set; }
        public string RequestText { get; set; }
        public bool IsCompleted {  get; set; }
        public DateTime Time {  get; set; }

        public static List<RequestCustomer> requestCustomers = new();
            
    }
}
