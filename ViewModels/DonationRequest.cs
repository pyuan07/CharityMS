using CharityMS.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CharityMS.ViewModels
{
    public class DonationRequest
    {
        public Guid Id { get; set; }
        public string ApplicantName { get; set; }
        public Guid ReceiverId { get; set; }
        public List<Item> Donations { get; set; }
        public string Reason { get; set; }
    }
}
