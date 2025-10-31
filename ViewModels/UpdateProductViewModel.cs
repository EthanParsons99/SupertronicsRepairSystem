using System.ComponentModel.DataAnnotations;
using SupertronicsRepairSystem.Models;

namespace SupertronicsRepairSystem.ViewModels
{
    // ViewModel for updating an existing product
    public class UpdateProductViewModel
    {
        [Required]
        public string Id { get; set; }

        [Required]
        [StringLength(100, ErrorMessage = "The name cant exceed 100 charecters")]
        public string Name { get; set; }

        [StringLength(500, ErrorMessage = "The desciption cant exceed 500 charecters")]
        public string Description { get; set; }

        [Required]
        [Url(ErrorMessage = "Please enter a valid URL")]
        public string ImageUrl { get; set; }

        [Required]
        [Range(0.01, double.MaxValue, ErrorMessage = "Price must be greater than zero.")]
        public double Price { get; set; }


        [Range(0.0, double.MaxValue, ErrorMessage = "Was Price cannot be negative.")]
        public double WasPrice { get; set; }

        [Range(0, 100, ErrorMessage = "Discount Percentage must be between 0 and 100.")]
        public int DiscountPercentage { get; set; }

        [Required]
        [Range(0, int.MaxValue, ErrorMessage = "Stock Quantity cannot be negative.")]
        public int StockQuantity { get; set; }

        [StringLength(50)]
        public string SerialNumber { get; set; }
    }
}
