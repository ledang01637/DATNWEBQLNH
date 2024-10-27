using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DATN.Shared
{
    public class RequestCustomer
    {
        public int RequestId { get; set; }
        public int TableNumbe { get; set; }
        public string RequestText { get; set; }
        public bool IsCompleted {  get; set; }

        public static List<RequestCustomer> requestCustomers = new();
            
    }
}
