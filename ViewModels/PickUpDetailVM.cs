using Amazon.S3.Model;
using CharityMS.Areas.Identity.Data;
using CharityMS.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace CharityMS.ViewModels
{
    public class PickUpDetailVM
    {
        public Guid Id { get; set; }

        [Display(Name = "Address")]
        public string Location { get; set; }

        [Display(Name = "Donor")]
        public User Donor { get; set; }

        [Required]
        [Display(Name = "Estimated Pick Up Date")]
        public DateTime EstimatiedPickUpDate { get; set; }


        [Display(Name = "Status")]
        public string Status { get; set; }

        [Display(Name = "Pick Up Date")]
        public DateTime PickUpDate { get; set; }

        [Display(Name = "Donations")]
        public List<Item> Donations { get; set; }

        public List<S3Object> images { get; set; }
    }
}
