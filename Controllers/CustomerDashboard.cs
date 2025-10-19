using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using SupertronicsRepairSystem.Models;


namespace SupertronicsRepairSystem.Controllers
{
    public class CustomerDashboard : Controller
    {
        public IActionResult Index()
        {
            // Simulate fetching data from a database or service
            var productsOnSale = new List<Product>
            {
                new Product
                {
                    Id = 1,
                    Name = "Acer Aspire 3",
                    Description = "15.6-inch FHD Laptop with Intel Core i7",
                    ImageUrl = "/images/acer.png", // Assuming image is in wwwroot/images
                    Price = 9999m,
                    WasPrice = 10999m,
                    DiscountPercentage = 9
                },
                new Product
                {
                    Id = 2,
                    Name = "JBL Live 770NC",
                    Description = "Noise Cancelling Wireless Headphones",
                    ImageUrl = "/images/jbl-headphones.png", // Assuming image is in wwwroot/images
                    Price = 1498m,
                    WasPrice = 2498m,
                    DiscountPercentage = 40
                }
            };

            // Pass the list of products to the view
            return View(productsOnSale);
        }
    }
}