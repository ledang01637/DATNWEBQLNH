using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DATN.Shared
{
    public class Floor
    {
        [Key]
        public int FloorId { get; set; }
        public int NumberFloor { get; set; }
        public bool IsDeleted { get; set; }

        public virtual ICollection<Table> Tables { get; set; }
    }
}
