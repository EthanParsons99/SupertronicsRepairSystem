using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;

namespace SupertronicsRepairSystem.ViewModels.Technician
{
    public class SelectJobForQuoteViewModel
    {
        [Required(ErrorMessage = "You must select a repair job.")]
        [Display(Name = "Select an Open Repair Job")]
        public string SelectedJobId { get; set; }

        public List<SelectListItem> OpenJobs { get; set; }
    }
}