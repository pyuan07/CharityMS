using CharityMS.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace CharityMS.ViewModels
{
    public class DonationVM
    {
        public Guid Id { get; set; }
        public Guid ReceiverId { get; set; }

        [Display(Name = "Receiver")]

        public string ReceiverFullName { get; set; }
        public List<Item> Donations { get; set; }

        public string Reason { get; set; }

        public string Status { get; set; }
        public Guid StaffId { get; set; }
        [Display(Name = "Staff")]

        public string StaffFullName { get; set; }

        [Display(Name = "Review Date")]

        public DateTime Date { get; set; }
    }
}
