//---------------------Customer DashboardController.cs---------------------
using Google.Cloud.Firestore;
using Microsoft.AspNetCore.Mvc;
using SupertronicsRepairSystem.Attributes;
using SupertronicsRepairSystem.Models;
using SupertronicsRepairSystem.Services;
using SupertronicsRepairSystem.ViewModels;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace SupertronicsRepairSystem.Controllers
{
    [AuthorizeCustomer] // Only logged in customers can access
    public class CustomerDashboardController : Controller
    {
        private readonly FirestoreDb _firestoreDb; // Firestore database
        private readonly CollectionReference _repairJobsCollection; // Repair jobs collection
        private readonly IAuthService _authService; // Auth service

        public CustomerDashboardController(FirestoreDb firestoreDb, IAuthService authService)
        {
            _firestoreDb = firestoreDb;
            _repairJobsCollection = _firestoreDb.Collection("repairJobs");
            _authService = authService;
        }

        public IActionResult CustomerViewProduct()
        {
            return View(); // Load product view
        }

        // GET: Load quote form
        [HttpGet]
        public async Task<IActionResult> CustomerGetQuote()
        {
            var model = new CustomerQuoteViewModel();
            {
                model.DeviceTypes = new List<string> { "Laptop", "Phone", "Console", "Tablet", "TV", "Other" };
            }
            ;

            var userInfo = await _authService.GetCurrentUserInfoAsync();
            if (userInfo != null)
            {
                model.Name = userInfo.FirstName;
                model.Email = userInfo?.Email;
                model.PhoneNumber = userInfo?.PhoneNumber;
            }

            return View(model); // Show form
        }

        // POST: Handle quote submission
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CustomerGetQuote(CustomerQuoteViewModel model)
        {
            model.DeviceTypes = new List<string> { "Laptop", "Phone", "Console", "Tablet", "TV", "Other" };

            if (!ModelState.IsValid)
            {
                return View(model); // Return if form invalid
            }

            var userId = await _authService.GetCurrentUserIdAsync();

            var repairJob = new RepairJob
            {
                ItemModel = $"{model.DeviceType} {model.Brand} {model.Model}".Trim(),
                SerialNumber = model.SerialNumber,
                ProblemDescription = model.ProblemDescription,
                Status = "Pending",
                DateReceived = Google.Cloud.Firestore.Timestamp.FromDateTime(System.DateTime.UtcNow),
                LastUpdated = Google.Cloud.Firestore.Timestamp.FromDateTime(System.DateTime.UtcNow),
                CustomerId = userId ?? string.Empty,
                CustomerName = string.IsNullOrEmpty(model.Name) ? model.Email ?? "Guest" : model.Name
            };

            await _repairJobsCollection.AddAsync(repairJob); // Save to Firestore

            TempData["SuccessMessage"] = "Quote request submitted. We will email you when the quote is ready.";
            return RedirectToAction(nameof(CustomerViewProduct)); // Go back to product page
        }

        public IActionResult CustomerCart()
        {
            return View(); // Load cart view
        }
    }
}
