using System.ComponentModel.DataAnnotations;
using System;
using System.Collections.Generic;


namespace SupertronicsRepairSystem.ViewModels
{
    public class CustomerQuoteViewModel
    {
        // Customer details
        [Required]
        [Display(Name = "First name")]
        public string Name { get; set; }

        [Display(Name = "Surname")]
        public string? Surname { get; set; }

        [Required]
        [EmailAddress]
        [Display(Name = "Email")]
        public string? Email { get; set; }

        [Phone]
        [Display(Name = "Cellphone Number")]
        public string? PhoneNumber { get; set; }

        // Device / job info
        [Required]
        [Display(Name = "Device type")]
        public string DeviceType { get; set; }

        public List<string> DeviceTypes { get; set; } = new List<string>();

        [Display(Name = "Brand / Manufacturer")]
        public string? Brand { get; set; }

        [Display(Name = "Model")]
        public string? Model { get; set; }

        [Display(Name = "Serial number")]
        public string? SerialNumber { get; set; }

        [Required]
        [Display(Name = "Problem description")]
        [StringLength(1000)]
        public string ProblemDescription { get; set; }
    }
}
