using Microsoft.AspNetCore.Mvc;
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
        [Phone(ErrorMessage = "Invalid phone number")]
        [Display(Name = "Customer Contact Number")]
        public string ContactNumber { get; set; }

        [Display(Name = "Customer ID/Passport Number")]
        public string IdPassportNumber { get; set; }

        [Required(ErrorMessage = "Device SKU is required")]
        [Display(Name = "Device Sku")]
        public string DeviceSku { get; set; }

        [Display(Name = "Collection Date")]
        [DataType(DataType.Date)]
        public DateTime? CollectionDate { get; set; }
    }

    public class ProductViewModel
    {
        public string Name { get; set; }
        public string Sku { get; set; }
        public string ImageUrl { get; set; }
    }
}
