using System;
using System.ComponentModel.DataAnnotations;

namespace SupertronicsRepairSystem.ViewModels
{
    public class KeepAsideViewModel
    {
        [Required(ErrorMessage = "Customer name is required")]
        [Display(Name = "Customer Name")]
        public string CustomerName { get; set; }

        [Required(ErrorMessage = "Customer surname is required")]
        [Display(Name = "Customer Surname")]
        public string CustomerSurname { get; set; }

        [Required(ErrorMessage = "Contact number is required")]
        [Display(Name = "Contact Number")]
        [RegularExpression(@"^[0-9]{10}$", ErrorMessage = "Contact number must be 10 digits")]
        public string ContactNumber { get; set; }

        [Required(ErrorMessage = "ID/Passport number is required")]
        [Display(Name = "ID/Passport Number")]
        public string IdPassportNumber { get; set; }

        [Required(ErrorMessage = "Device serial number is required")]
        [Display(Name = "Device Serial Number")]
        public string DeviceSerialNumber { get; set; }

        [Required(ErrorMessage = "Collection date is required")]
        [Display(Name = "Collection Date")]
        [DataType(DataType.Date)]
        public DateTime? CollectionDate { get; set; }

        public string? ImageUrl { get; set; }
        public string? CustomerId { get; set; }
    }
}