using Google.Cloud.Firestore;
using System.ComponentModel.DataAnnotations;    

namespace SupertronicsRepairSystem.ViewModels
{
    public class AddProductViewModel
    {
        [Required]
        [StringLength(100, ErrorMessage = "The Name field cannot exceed 100 characters.")]
        public string Name { get; set; }
        [Required]
        [StringLength(500, ErrorMessage = "The Description field cannot exceed 500 characters.")]
        public string Description { get; set; }
        [Required]
        [Url(ErrorMessage = "Please enter a valid URL for the Image.")]
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


    }
}