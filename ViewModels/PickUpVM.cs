using CharityMS.Areas.Identity.Data;
using CharityMS.Models;
using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace CharityMS.ViewModels
{
    public class PickUpVM
    {
        [Required]
        [Display(Name = "Id")]
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

        public List<SelectListItem> StatusType { get; } = new List<SelectListItem>
        {
            new SelectListItem { Value = "Picked-Up", Text = "Picked-Up" },
            new SelectListItem { Value = "Cancelled", Text = "Cancelled" },
            new SelectListItem { Value = "Scheduled", Text = "Scheduled" },
            new SelectListItem { Value = "Pending", Text = "Pending" }
        };
    }
}
