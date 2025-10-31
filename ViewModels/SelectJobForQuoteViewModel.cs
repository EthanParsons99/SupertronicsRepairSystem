using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;

namespace SupertronicsRepairSystem.ViewModels.Technician
{
    // ViewModel for selecting a job to create a quote for  repairs
    // Contains a list of open repair jobs and the selected job ID
    public class SelectJobForQuoteViewModel
    {
        [Required(ErrorMessage = "You must select a repair job.")]
        [Display(Name = "Select an Open Repair Job")]
        public string SelectedJobId { get; set; }

        public List<SelectListItem> OpenJobs { get; set; }
    }
}