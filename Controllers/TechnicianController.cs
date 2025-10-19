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
            return View();
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
