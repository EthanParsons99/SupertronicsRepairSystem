using Microsoft.AspNetCore.Mvc;

namespace SupertronicsRepairSystem.Controllers
{
    public class OwnerController : Controller
    {
        // GET: Owner/Dashboard
        public IActionResult Dashboard()
        {
            return View();
        }

        // GET: Owner/Index (alternative route)
        public IActionResult Index()
        {
            return RedirectToAction("Dashboard");
        }

        // GET: Owner/ProductManagement
        public IActionResult ProductManagement()
        {
            return View();
        }

        // GET: Owner/AddProduct
        public IActionResult AddProduct()
        {
            return View();
        }

        // GET: Owner/QuotesManagement
        public IActionResult QuotesManagement()
        {
            return View();
        }

        // GET: Owner/RepairJobs
        public IActionResult RepairJobs()
        {
            return View();
        }

        // GET: Owner/TechnicianManagement
        public IActionResult TechnicianManagement()
        {
            return View();
        }

        // GET: Owner/AddTechnician
        public IActionResult AddTechnician()
        {
            return View();
        }
    }
}