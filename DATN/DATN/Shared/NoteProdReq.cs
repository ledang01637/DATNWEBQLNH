using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DATN.Shared
{
    public class NoteProdReq
    {
        public string TableNumber { get; set; }
        public List<ProdReq> ProdReqs { get; set; } = new();
        public string Note { get; set; }
        public static List<NoteProdReq> noteProdReqs {  get; set; } = new();
    }
}
