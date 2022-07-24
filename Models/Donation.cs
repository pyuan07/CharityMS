using CharityMS.Areas.Identity.Data;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace CharityMS.Models
{
    public class Donation
    {
        [Key]
        public Guid Id { get; set; }
        public Guid ReceiverId { get; set; }
        public List<Item> Donations { get; set; }

        public string Reason { get; set; }

        public string Status { get; set; }
        public Guid StaffId { get; set; }
        public DateTime Date { get; set; }

    }
}
