using Microsoft.AspNetCore.Mvc;
using SupertronicsRepairSystem.ViewModels;

namespace SupertronicsRepairSystem.Controllers
{
    public class KeepAsideController : Controller
    {
        // GET: KeepAside
        public IActionResult Index()
        {
            var model = new KeepAsideViewModel
            {
                CollectionDate = DateTime.Now.AddDays(7) // Default to 7 days from now
            };

            // Sample product data
            ViewBag.Product = new ProductViewModel
            {
                Name = "Acer Aspire 3 A315-59-75MV 15.6-inch FHD Laptop Intel Core i7-1255U",
                Sku = "3082-1222",
                ImageUrl = "/images/laptop.jpg"
            };

            return View(model);
        }

        // POST: KeepAside/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(KeepAsideViewModel model)
        {
            if (ModelState.IsValid)
            {
              

                TempData["SuccessMessage"] = "Keep aside created successfully!";
                return RedirectToAction("Success");
            }

          
            ViewBag.Product = new ProductViewModel
            {
                Name = "Acer Aspire 3 A315-59-75MV 15.6-inch FHD Laptop Intel Core i7-1255U",
                Sku = "3082-1222",
                ImageUrl = "/images/laptop.jpg"
            };

            return View("Index", model);
        }

        // GET: KeepAside/Success
        public IActionResult Success()
        {
            return View();
        }

        // POST: KeepAside/Cancel
        [HttpPost]
        public IActionResult Cancel()
        {
            return RedirectToAction("Index", "Home");
        }

    }
}
