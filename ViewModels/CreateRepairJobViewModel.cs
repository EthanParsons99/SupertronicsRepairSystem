using System.ComponentModel.DataAnnotations;

namespace SupertronicsRepairSystem.ViewModels.Technician
{
    // Customer-facing view model for creating repair requests (used by Technician views/controllers)
    public class CreateRepairJobViewModel
    {
        [Required(ErrorMessage = "Item model is required")]
        [Display(Name = "Item Model")]
        public string ItemModel { get; set; }

        [Display(Name = "Serial Number")]
        public string? SerialNumber { get; set; }

        [Required(ErrorMessage = "Problem description is required")]
        [Display(Name = "Problem Description")]
        [MinLength(10, ErrorMessage = "Please provide a detailed description (minimum 10 characters)")]
        public string ProblemDescription { get; set; }

        [Display(Name = "Customer Name")]
        public string? CustomerName { get; set; }
    }
}