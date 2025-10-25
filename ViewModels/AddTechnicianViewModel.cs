using System.ComponentModel.DataAnnotations;
namespace SupertronicsRepairSystem.ViewModels
{
    public class AddTechnicianViewModel
    {
        [Required(ErrorMessage = "Technician name is required.")]
        [StringLength(100, ErrorMessage = "Technician name cannot exceed 100 characters.")]
        public string FirstName { get; set; }

        [Required(ErrorMessage = "Surname is required.")]
        [StringLength(100, ErrorMessage = "Surname cannot exceed 100 characters.")]
        public string Surname{ get; set; }

        [Required(ErrorMessage = "Email address is required.")]
        [EmailAddress(ErrorMessage = "Invalid email address format.")]
        public string Email { get; set; }

        [Required(ErrorMessage = "Phone number is required.")]
        [Phone(ErrorMessage = "Invalid phone number format.")]
        public string PhoneNumber { get; set; }

        [Required(ErrorMessage = "Password is required.")]
        [DataType(DataType.Password)]
        public string Password { get; set; }
    }
}
