using Google.Cloud.Firestore;
using System;
using System.ComponentModel.DataAnnotations;

namespace SupertronicsRepairSystem.Models
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
        [Phone(ErrorMessage = "Invalid phone number")]
        [Display(Name = "Customer Contact Number")]
        public string ContactNumber { get; set; }

        [Display(Name = "Customer ID/Passport Number")]
        public string IdPassportNumber { get; set; }

        [Required(ErrorMessage = "Device Serial Number is required")]
        [Display(Name = "Device Serial Number")]
        public string DeviceSerialNumber { get; set; }

        [Display(Name = "Collection Date")]
        [DataType(DataType.Date)]
        public DateTime? CollectionDate { get; set; }

        public string ImageUrl { get; set; }
    }
}
