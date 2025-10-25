using Microsoft.AspNetCore.Mvc;
using SupertronicsRepairSystem.Attributes;
using SupertronicsRepairSystem.Services;
using SupertronicsRepairSystem.ViewModels;
using SupertronicsRepairSystem.ViewModels.Technician;

namespace SupertronicsRepairSystem.Controllers
{
    public class CustomerController : Controller
    {
        private readonly IAuthService _authService;
        private readonly IRepairJobService _repairJobService;

        public CustomerController(IAuthService authService, IRepairJobService repairJobService)
        {
            _authService = authService;
            _repairJobService = repairJobService;
        }

        public async Task<IActionResult> Dashboard()
        {
            var userInfo = await _authService.GetCurrentUserInfoAsync();
            ViewBag.UserInfo = userInfo;

            // Get customer's repair jobs for dashboard
            if (userInfo != null)
            {
                var repairJobs = await _repairJobService.GetRepairJobsByCustomerIdAsync(userInfo.UserId);
                ViewBag.RepairJobs = repairJobs;
            }

            return View();
        }

        public async Task<IActionResult> TrackRepair()
        {
            var userInfo = await _authService.GetCurrentUserInfoAsync();

            if (userInfo != null)
            {
                var repairJobs = await _repairJobService.GetRepairJobsByCustomerIdAsync(userInfo.UserId);
                return View(repairJobs);
            }

            return View(new List<SupertronicsRepairSystem.Models.RepairJob>());
        }

        // Changed method name to match the view file
        public IActionResult CustomerViewProduct()
        {
            return View();
        }

        // GET: Customer/RequestRepair
        public IActionResult RequestRepair()
        {
            return View(new CreateRepairJobViewModel());
        }

        // POST: Customer/RequestRepair
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RequestRepair(CreateRepairJobViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var userInfo = await _authService.GetCurrentUserInfoAsync();

            if (userInfo == null)
            {
                TempData["Error"] = "You must be logged in to request a repair.";
                return RedirectToAction("SignIn", "Account");
            }

            try
            {
                var customerName = $"{userInfo.FirstName} {userInfo.Surname}".Trim();
                if (string.IsNullOrEmpty(customerName))
                {
                    customerName = userInfo.Email;
                }

                var repairJobId = await _repairJobService.CreateRepairJobAsync(
                    model,
                    userInfo.UserId,
                    customerName
                );

                TempData["Success"] = $"Repair request submitted successfully! Your job ID is: {repairJobId.Substring(0, Math.Min(10, repairJobId.Length))}...";
                return RedirectToAction("TrackRepair");
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, "An error occurred while submitting your repair request. Please try again.");
                return View(model);
            }
        }

        // GET: Customer/RepairDetails/{id}
        public async Task<IActionResult> RepairDetails(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return NotFound();
            }

            var userInfo = await _authService.GetCurrentUserInfoAsync();
            var repairJob = await _repairJobService.GetRepairJobByIdAsync(id);

            if (repairJob == null)
            {
                return NotFound();
            }

            // Ensure customer can only view their own repair jobs
            if (userInfo != null && repairJob.CustomerId != userInfo.UserId)
            {
                return Forbid();
            }

            return View(repairJob);
        }
    }
}