using CharityMS.Areas.Identity.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CharityMS.Models
{
    public class Donation
    {
        public Guid Id { get; set; }

        public string ReceiverName;

        public User Staff { get; set; }

        public DateTime Date { get; set; }

        public List<string> Donations { get; set; }
    }
}
