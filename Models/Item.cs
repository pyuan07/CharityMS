using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace CharityMS.Models
{
    public class Item
    {
        [Key]
        public Guid Id { get; set; }
        public string ItemName { get; set; }
        public int Quantity { get; set; }

        public override string ToString()
        {
            return $"[{ItemName} x {Quantity}]";
        }
    }
}
