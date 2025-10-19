using Microsoft.AspNetCore.Mvc;
using SupertronicsRepairSystem.Models;
using SupertronicsRepairSystem.ViewModels.Technician;

namespace SupertronicsRepairTrackingSystem.Controllers
{
    public class TechnicianController : Controller
    {
        public IActionResult Dashboard()
        {
            return View();
        }

        public IActionResult RepairJobs()
        {

            // Sample data for demo
            var repairJobs = new List<RepairJobViewModel>
            {
                new RepairJobViewModel
                {
                    JobId = 1001,
                    ItemName = "Dell XPS 15 Laptop",
                    Status = "In Progress",
                    CustomerName = "John Smith",
                    LastUpdated = DateTime.Now.AddDays(-2)
                },
                new RepairJobViewModel
                {
                    JobId = 1002,
                    ItemName = "iPhone 14 Pro",
                    Status = "Pending",
                    CustomerName = "Sarah Johnson",
                    LastUpdated = DateTime.Now.AddHours(-5)
                },
                new RepairJobViewModel
                {
                    JobId = 1003,
                    ItemName = "Samsung Galaxy S23",
                    Status = "Completed",
                    CustomerName = "Michael Brown",
                    LastUpdated = DateTime.Now.AddDays(-7)
                }
            };

            return View(repairJobs);
        }


        public IActionResult CreateRepairJob()
        {
            return View();
        }

        public IActionResult GenerateRepairQuote()
        {
            return View();
        }

        public IActionResult GenerateProductQuote()
        {
            return View();
        }

        public IActionResult TrackSerialNumber()
        {
            return View();
        }
    }
}
