using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SupertronicsRepairSystem.Attributes;
using SupertronicsRepairSystem.Services;
using SupertronicsRepairSystem.ViewModels;
using System.Threading.Tasks;

namespace SupertronicsRepairSystem.Controllers
{
    [AuthorizeRole(UserRole.Owner)]
    public class OwnerController : Controller
    {
        private readonly IAuthService _authService;
        public OwnerController(IAuthService authService)
        {
            _authService = authService;
        }
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
            return View(new AddTechnicianViewModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddTechnician(AddTechnicianViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var result = await _authService.SignUpAsync(
                model.Email,
                model.Password,
                model.FirstName,
                model.Surname,
                model.PhoneNumber,
                UserRole.Technician
            );

            if (result.Success)
            {
                TempData["Success"] = "Technician added successfully.";
                return RedirectToAction("Dashboard");
            }
            else
            {
                ModelState.AddModelError(string.Empty, result.Message);
                return View(model);

            }
        }
    }
}