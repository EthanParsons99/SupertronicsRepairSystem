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

using Google.Cloud.Firestore;
using Microsoft.AspNetCore.Mvc;
using SupertronicsRepairSystem.Attributes;
using SupertronicsRepairSystem.Models;
using SupertronicsRepairSystem.Services;
using SupertronicsRepairSystem.ViewModels.Technician;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace SupertronicsRepairSystem.Controllers
{
    [AuthorizeCustomer]
    public class CustomerDashboard : Controller
    {
        private readonly FirestoreDb _firestoreDb;
        private readonly CollectionReference _repairJobsCollection;
        private readonly IAuthService _authService;

        public CustomerDashboard(FirestoreDb firestoreDb, IAuthService authService)
        {
            _firestoreDb = firestoreDb;
            _repairJobsCollection = _firestoreDb.Collection("repairJobs");
            _authService = authService;
        }

        // Existing view for product listing (keeps prior behavior)
        public IActionResult CustomerViewProduct()
        {
            return View();
        }

        // GET: Show the customer quote request form
        [HttpGet]
        public async Task<IActionResult> CustomerGetQuote()
        {
            var model = new GenerateRepairQuoteViewModel();
            {
                GenerateDeviceName = new List<string> { "Laptop", "Phone", "Console", "Tablet", "TV", "Other" }
            };

            var userInfo = await _authService.GetCurrentUserInfoAsync();
            if (userInfo != null)
            {
                model.CustomerName = userInfo.FirstName;
                model.CustomerEmail = userInfo?.Email;
                model.CustomerPhone = userInfo?.PhoneNumber;
            }

            return View(model);
        }

        // POST: Submit the quote request -> create a RepairJob (status: Pending)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CustomerGetQuote(GenerateRepairQuoteViewModel model)
        {
            model.DeviceName = new List<string> { "Laptop", "Phone", "Console", "Tablet", "TV", "Other" };

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var userId = await _authService.GetCurrentUserIdAsync();
            var customerName = string.IsNullOrWhiteSpace(model.Surname)
                ? model.CustomerName?.Trim()
                : $"{model.Name} {model.Surname}".Trim();

            var repairJob = new RepairJob
            {
                ItemModel = $"{model.DeviceName} {model.Brand} {model.Model}".Trim(),
                SerialNumber = model.SerialNumber,
                ProblemDescription = model.ProblemDescription,
                Status = "Pending",
                DateReceived = Google.Cloud.Firestore.Timestamp.FromDateTime(System.DateTime.UtcNow),
                LastUpdated = Google.Cloud.Firestore.Timestamp.FromDateTime(System.DateTime.UtcNow),
                CustomerId = userId ?? string.Empty,
                CustomerName = string.IsNullOrEmpty(customerName) ? model.CustomerEmail ?? "Guest" : customerName
            };

            await _repairJobsCollection.AddAsync(repairJob);

            TempData["SuccessMessage"] = "Quote request submitted. We will email you when the quote is ready.";
            return RedirectToAction(nameof(CustomerViewProduct));
        }

        public IActionResult CustomerCart()
        {
            return View();
        }
    }
}