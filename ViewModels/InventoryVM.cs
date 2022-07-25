using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace CharityMS.ViewModels
{
    public class InventoryVM
    {
        [Required]
        public string DonorID { get; set; }

        public string InventoryID { get; set; }

        [Required]
        public string Name { get; set; }

        [Required]
        public string Quantity { get; set; }

        public DateTime DonationDate { get; set; }

        public string ExpiredDate { get; set; }

        [Required]
        public string Category { get; set; }

        public string ProductCondition { get; set; }

        public string cloteSize { get; set; }

        public string Receiver { get; set; }

        public string Status { get; set; }

        public List<SelectListItem> Categories { get; } = new List<SelectListItem>
        {
            new SelectListItem { Value = "", Text = "Select" },
            new SelectListItem { Value = "food", Text = "Food" },
            new SelectListItem { Value = "beverage", Text = "Beverage" },
            new SelectListItem { Value = "electronics", Text = "Electronics"  },
            new SelectListItem { Value = "clothe", Text = "Clothe"  }
        };

        public List<SelectListItem> Conditions { get; } = new List<SelectListItem>
        {
            new SelectListItem { Value = "", Text = "Select" },
            new SelectListItem { Value = "New", Text = "Brand New" },
            new SelectListItem { Value = "good", Text = "Good" },
            new SelectListItem { Value = "bad", Text = "Bad"  },
            new SelectListItem { Value = "rubbish", Text = "Rubbish"  }
        };

        public List<SelectListItem> clothSizes { get; } = new List<SelectListItem>
        {
            new SelectListItem { Value = "", Text = "Select" },
            new SelectListItem { Value = "S", Text = "S" },
            new SelectListItem { Value = "M", Text = "M" },
            new SelectListItem { Value = "L", Text = "L"  },
            new SelectListItem { Value = "XM", Text = "XL"  }
        };

        public List<SelectListItem> Statusddl { get; } = new List<SelectListItem>
        {
            new SelectListItem { Value = "Available", Text = "Available" },
            new SelectListItem { Value = "Not Available", Text = "Not Available" },
            new SelectListItem { Value = "Donated", Text = "Donated"  },
        };
    }
}
