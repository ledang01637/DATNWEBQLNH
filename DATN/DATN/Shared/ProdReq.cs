using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DATN.Shared
{
    public class ProdReq
    {
        public int ProductId { get; set; }
        public string ProductName { get; set; }
        public string UnitName { get; set; }
        public int Quantity { get; set; }
        public int CompletedQuantity { get; set; }
        public bool IsComplete => CompletedQuantity >= Quantity;
    }
}
