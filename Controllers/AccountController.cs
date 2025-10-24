using Microsoft.AspNetCore.Mvc;
using SupertronicsRepairSystem.Services;
using System.Threading.Tasks;

namespace SupertronicsRepairSystem.Controllers
{
    public class AccountController : Controller
    {
        private readonly IAuthService _authService;

        public AccountController(IAuthService authService)
        {
            _authService = authService;
        }

        [HttpGet]
        public async Task<IActionResult> Login(string returnUrl = null)
        {
            if (await _authService.IsAuthenticatedAsync())
            {
                return RedirectToAction("Index", "Home");
            }

            ViewData["ReturnUrl"] = returnUrl;
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(string email, string password, bool rememberMe, string returnUrl = null)
        {
            if (await _authService.IsAuthenticatedAsync())
            {
                return RedirectToAction("Index", "Home");
            }

            if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
            {
                TempData["Error"] = "Email and password are required";
                return View();
            }

            var result = await _authService.SignInAsync(email, password, rememberMe);

            if (result.Success)
            {
                if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
                {
                    return Redirect(returnUrl);
                }

                // Redirect based on role
                return result.Role switch
                {
                    UserRole.Owner => RedirectToAction("Dashboard", "Owner"),
                    UserRole.Technician => RedirectToAction("Dashboard", "Technician"),
                    UserRole.Customer => RedirectToAction("CustomerViewProduct", "Customer"),
                    _ => RedirectToAction("Index", "Home")
                };
            }

            TempData["Error"] = result.Message;
            return View();
        }

        [HttpGet]
        public async Task<IActionResult> Register()
        {
            if (await _authService.IsAuthenticatedAsync())
            {
                return RedirectToAction("Index", "Home");
            }

            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(string email, string password, string confirmPassword)
        {
            if (await _authService.IsAuthenticatedAsync())
            {
                return RedirectToAction("Index", "Home");
            }

            if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
            {
                TempData["Error"] = "Email and password are required";
                return View();
            }

            if (password != confirmPassword)
            {
                TempData["Error"] = "Passwords do not match";
                return View();
            }

            // New registrations default to Customer role
            var result = await _authService.SignUpAsync(email, password, UserRole.Customer);

            if (result.Success)
            {
                TempData["Success"] = "Account created successfully. Please sign in.";
                return RedirectToAction("Login");
            }

            TempData["Error"] = result.Message;
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            await _authService.SignOutAsync();
            return RedirectToAction("Login");
        }

        [HttpGet]
        public async Task<IActionResult> ForgotPassword()
        {
            if (await _authService.IsAuthenticatedAsync())
            {
                return RedirectToAction("Index", "Home");
            }

            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ForgotPassword(string email)
        {
            if (string.IsNullOrWhiteSpace(email))
            {
                TempData["Error"] = "Email is required";
                return View();
            }

            var result = await _authService.ResetPasswordAsync(email);

            if (result.Success)
            {
                TempData["Success"] = "Password reset email has been sent. Please check your inbox.";
                return RedirectToAction("Login");
            }

            TempData["Error"] = result.Message;
            return View();
        }
    }
}