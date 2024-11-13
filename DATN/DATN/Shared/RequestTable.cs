
using System.Collections.Generic;

namespace DATN.Shared
{
    public class RequestTable
    {
        public int TableId {  get; set; }
        public int FloorId { get; set; }
        public int NumberTable { get; set; }
        public List<string> Requests { get; set; } = new();
        public bool IsCompleted { get; set; } = false;
        public string ButtonClass => IsCompleted ? "completed" : "processing";
        
    }
}
