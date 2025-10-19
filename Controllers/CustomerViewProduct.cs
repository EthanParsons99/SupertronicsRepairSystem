using Microsoft.AspNetCore.Mvc;

namespace SupertronicsRepairSystem.Controllers
{
    public class CustomerDashboardController : Controller
    {
        public IActionResult CustomerViewProduct()
        {
            return View();
        }
    }

}

