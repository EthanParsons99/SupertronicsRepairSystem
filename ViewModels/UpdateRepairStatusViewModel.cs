using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;

namespace SupertronicsRepairSystem.ViewModels.Technician
{
    public class UpdateRepairStatusViewModel
    {
        public string JobId { get; set; }
        public string ItemModel { get; set; }
        public string CustomerName { get; set; }
        public string CurrentStatus { get; set; }

        [Required(ErrorMessage = "Please select a new status.")]
        [Display(Name = "New Status")]
        public string NewStatus { get; set; }

        public List<SelectListItem> StatusOptions { get; set; } = new List<SelectListItem>
        {
            new SelectListItem { Value = "Pending", Text = "Pending" },
            new SelectListItem { Value = "Diagnosis", Text = "Diagnosis" },
            new SelectListItem { Value = "In Progress", Text = "In Progress" },
            new SelectListItem { Value = "Completed", Text = "Completed" },
            new SelectListItem { Value = "Cancelled", Text = "Cancelled" }
        };
    }
}