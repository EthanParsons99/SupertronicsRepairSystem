using Microsoft.AspNetCore.Mvc;
using SupertronicsRepairSystem.Attributes;
using SupertronicsRepairSystem.Services;

namespace SupertronicsRepairSystem.Controllers
{
    [AuthorizeCustomer] // Only customers can access this controller
    public class CustomerController : Controller
    {
        private readonly IAuthService _authService;

        public CustomerController(IAuthService authService)
        {
            _authService = authService;
        }

        public async Task<IActionResult> Dashboard()
        {
            var userInfo = await _authService.GetCurrentUserInfoAsync();
            ViewBag.UserInfo = userInfo;
            return View();
        }

        public IActionResult TrackRepair()
        {
            return View();
        }

        public IActionResult ViewProducts()
        {
            return View();
        }

        public IActionResult RequestRepair()
        {
            return View();
        }
    }
}