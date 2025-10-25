//using System.Diagnostics;
//using Microsoft.AspNetCore.Mvc;
//using SupertronicsRepairSystem.Models.CustomerModel;


//namespace SupertronicsRepairSystem.Controllers
//{
//    public class CustomerDashboard : Controller
//    {
//        public IActionResult Index()
//        {
//            // Simulate fetching data from a owners dashboard, will update it once i get calwyns part
//            var productsOnSale = new List<Product>
//            {
//                new Product
//                {
//                    Id = 1,
//                    Name = "Acer Aspire 3",
//                    Description = "15.6-inch FHD Laptop with Intel Core i7",
//                    ImageUrl = "/images/acer.png", 
//                    Price = 9999m,
//                    WasPrice = 10999m,
//                    DiscountPercentage = 9
//                },
//                new Product
//                {
//                    Id = 2,
//                    Name = "JBL Live 770NC",
//                    Description = "Noise Cancelling Wireless Headphones",
//                    ImageUrl = "/images/jbl-headphones.png",
//                    WasPrice = 2498m,
//                    DiscountPercentage = 40
//                },
//                 new Product
//                {
//                    Id = 3,
//                    Name = "Acer Aspire 3",
//                    Description = "15.6-inch FHD Laptop with Intel Core i7",
//                    ImageUrl = "/images/acer.png",
//                    Price = 9999m,
//                    WasPrice = 10999m,
//                    DiscountPercentage = 9
//                },
//                  new Product
//                {
//                    Id = 4,
//                    Name = "Acer Aspire 3",
//                    Description = "15.6-inch FHD Laptop with Intel Core i7",
//                    ImageUrl = "/images/acer.png", 
//                    Price = 9999m,
//                    WasPrice = 10999m,
//                    DiscountPercentage = 9
//                },
//                   new Product
//                {
//                    Id = 5,
//                    Name = "Acer Aspire 3",
//                    Description = "15.6-inch FHD Laptop with Intel Core i7",
//                    ImageUrl = "/images/acer.png", 
//                    Price = 9999m,
//                    WasPrice = 10999m,
//                    DiscountPercentage = 9
//                },
//                    new Product
//                {
//                    Id = 6,
//                    Name = "Acer Aspire 3",
//                    Description = "15.6-inch FHD Laptop with Intel Core i7",
//                    ImageUrl = "/images/acer.png", 
//                    Price = 9999m,
//                    WasPrice = 10999m,
//                    DiscountPercentage = 9
//                },

//            };

          
//            return View(productsOnSale);
//        }
//    }
//}