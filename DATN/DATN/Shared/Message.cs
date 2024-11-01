using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DATN.Shared
{
    public class Message
    {
        [Key]
        public int MessageId {  get; set; }
        public int? TableId {  get; set; }
        public int? AccountId {  get; set; }
        public string Note {  get; set; }
        public string MessageText {  get; set; }
        public DateTime CreateDate { get; set; }
        public DateTime UpdateDate { get; set; }

        [NotMapped]
        public List<CartDTO> Carts { get; set; } = new();
        public virtual Account Account { get; set; }
        public virtual Table Table { get; set; }
    }
}
 