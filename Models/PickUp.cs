using CharityMS.Areas.Identity.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CharityMS.Models
{
    public class PickUp
    {
        public Guid id { get; set; }

        public User donor { get; set; }

        public User pickUpStaff { get; set; }

        public string location { get; set; }

        public DateTime pickUpDate { get; set; }

        public List<string> donations { get; set; }

        public string status { get; set; }//change to enum

    }
}
