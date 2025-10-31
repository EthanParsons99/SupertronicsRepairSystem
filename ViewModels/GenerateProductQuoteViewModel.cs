using System.ComponentModel.DataAnnotations;

namespace SupertronicsRepairSystem.ViewModels.Technician
{
    // ViewModel for generating a product quote
    // Includes product ID, quantity, and total price
    public class GenerateProductQuoteViewModel
    {
        [Required(ErrorMessage = "Product ID is required.")]
        [Display(Name = "Product ID")]
        public string ProductId { get; set; }

        [Required]
        [Range(1, 100, ErrorMessage = "Quantity must be between 1 and 100.")]
        public int Quantity { get; set; } = 1;

        public decimal TotalPrice { get; set; }
    }
}