using CharityMS.Areas.Identity.Data;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace CharityMS.Models
{
    public class PickUp
    {
        [Key]
        public Guid Id { get; set; }

        public Guid DonorId { get; set; }

        public Guid StaffId { get; set; }

        public string Location { get; set; }

        public DateTime EstimatedPickUpDate { get; set; }

        public DateTime PickUpDate { get; set; }

        public List<Item> Donations { get; set; }

        public string Status { get; set; }//change to enum

    }
}
