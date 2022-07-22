using CharityMS.Areas.Identity.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CharityMS.Models
{
    public class PickUp
    {
        public Guid Id { get; set; }

        public User Donor { get; set; }

        public User PickUpStaff { get; set; }

        public string Location { get; set; }

        public DateTime PickUpDate { get; set; }

        public List<string> Donations { get; set; }

        public string Status { get; set; }//change to enum

    }
}
