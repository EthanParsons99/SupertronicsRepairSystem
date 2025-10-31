using SupertronicsRepairSystem.Models;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
namespace SupertronicsRepairSystem.ViewModels
{
    // ViewModel for displaying and filtering a list of products
    public class ProductListViewModel
    {
        [Display(Name = "Search Name/Serial")]
        public string? SearchTerm { get; set; }

        [Display(Name = "Min Price")]
        [Range(0, double.MaxValue, ErrorMessage = "Min Price cannot be negative.")]
        public double? MinPrice { get; set; } = 0;

        [Display(Name = "Max Price")]
        [Range(0, double.MaxValue, ErrorMessage = "Max Price cannot be negative.")]
        public double? MaxPrice { get; set; }
        public List<Product> Products { get; set; } = new List<Product>();

    }
}
