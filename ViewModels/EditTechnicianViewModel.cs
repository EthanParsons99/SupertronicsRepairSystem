using SupertronicsRepairSystem.Models;
using SupertronicsRepairSystem.Services;
using System.ComponentModel.DataAnnotations;

namespace SupertronicsRepairSystem.ViewModels
{
    public class EditTechnicianViewModel
    {
        public string UserId { get; set; }

        [Required]
        [Display(Name = "First Name")]
        public string FirstName { get; set; }

        [Required]
        [Display(Name = "Surname")]
        public string Surname { get; set; }

        [Required]
        [EmailAddress]
        [Display(Name = "Email")]
        public string Email { get; set; }

        [Phone]
        [Display(Name = "Phone Number")]

        public string PhoneNumber { get; set; }

        public UserRole Role { get; set; } = UserRole.Technician;
    }
}
