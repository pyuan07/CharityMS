using CharityMS.Areas.Identity.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CharityMS.Models
{
    public class Donation
    {
        public Guid id { get; set; }

        public string receiverName;

        public User Staff { get; set; }

        public DateTime date { get; set; }

        public List<string> donations { get; set; }
    }
}
