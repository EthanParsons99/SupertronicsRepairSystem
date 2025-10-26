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
        private readonly IProductService _productService;
        private readonly IQuoteService _quoteService;
        private readonly IRepairJobService _repairJobService;

        public OwnerController(
            IAuthService authService,
            IProductService productService,
            IQuoteService quoteService,
            IRepairJobService repairJobService)
        {
            _authService = authService;
            _productService = productService;
            _quoteService = quoteService;
            _repairJobService = repairJobService;
        }

        // GET: Owner/TestRepairJobs
        public async Task<IActionResult> TestRepairJobs()
        {
            try
            {
                var repairJobs = await _repairJobService.GetAllRepairJobsAsync();

                var debugInfo = new
                {
                    JobCount = repairJobs?.Count ?? 0,
                    Jobs = repairJobs?.Select(j => new
                    {
                        j.Id,
                        j.ItemModel,
                        j.CustomerName,
                        j.Status,
                        j.SerialNumber,
                        j.ProblemDescription,
                        DateReceived = j.DateReceived != null ? j.DateReceived.ToDateTime().ToString() : "NULL",
                        LastUpdated = j.LastUpdated != null ? j.LastUpdated.ToDateTime().ToString() : "NULL",
                        HasQuotes = j.Quotes?.Count ?? 0,
                        HasNotes = j.TechnicianNotes?.Count ?? 0
                    }).ToList()
                };

                return Json(debugInfo);
            }
            catch (Exception ex)
            {
                return Json(new { error = ex.Message, stackTrace = ex.StackTrace });
            }
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
        public async Task<IActionResult> ProductManagement()
        {
            var products = await _productService.GetAllProductsAsync();
            var viewModel = new ProductListViewModel
            {
                Products = products
            };
            return View(viewModel);
        }

        public async Task<IActionResult> EditProduct(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return NotFound();
            }

            var product = await _productService.GetProductByIdAsync(id);
            if (product == null)
            {
                TempData["Error"] = "Product not found.";
                return RedirectToAction("ProductManagement");
            }

            var model = new UpdateProductViewModel
            {
                Id = product.Id,
                Name = product.Name,
                Description = product.Description,
                ImageUrl = product.ImageUrl,
                Price = product.Price,
                WasPrice = product.WasPrice,
                DiscountPercentage = product.DiscountPercentage,
                StockQuantity = product.StockQuantity,
                SerialNumber = product.SerialNumber
            };
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditProduct(UpdateProductViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }
            if(await _productService.UpdateProductAsync(model))
            {
                TempData["Success"] = "Product updated successfully.";
                return RedirectToAction("ProductManagement");
            }
            else
            {
                ModelState.AddModelError(string.Empty, "An error occurred while updating the product. Please try again.");
                return View(model);
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteProduct(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                TempData["Error"] = "Invalid product ID.";
                return RedirectToAction("ProductManagement");
            }

            var product = await _productService.GetProductByIdAsync(id);
            var productName = product?.Name ?? "Product";

            if (await _productService.DeleteProductAsync(id))
            {
                TempData["Success"] = $"{productName} deleted successfully.";
                return RedirectToAction("ProductManagement");
            }
            else
            {
                TempData["Error"] = $"An error occurred while deleting {productName}. Please try again.";
                return RedirectToAction("ProductManagement");
            }
        }

        // GET: Owner/AddProduct
        public IActionResult AddProduct()
        {
            return View(new AddProductViewModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddProduct(AddProductViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }
            var success = await _productService.AddProductAsync(model);

            if (success)
            {
                TempData["Success"] = "Product added successfully.";
                return RedirectToAction("ProductManagement");
            }
            else
            {
                ModelState.AddModelError(string.Empty, "An error occurred while adding the product. Please try again.");
                return View(model);
            }
        }

        // GET: Owner/QuotesManagement
        public async Task<IActionResult> QuotesManagement()
        {
            var repairJobsWithQuotes = await _quoteService.GetAllRepairJobsWithQuotesAsync();
            return View(repairJobsWithQuotes);
        }



        // POST: Owner/FilterQuotes
        [HttpPost]
        public async Task<IActionResult> FilterQuotes(string status, string customerId, DateTime? startDate, DateTime? endDate)
        {
            var filteredJobs = await _quoteService.GetFilteredRepairJobsAsync(status, customerId, startDate, endDate);
            return PartialView("_QuotesTable", filteredJobs);
        }

        // GET: Owner/RepairJobs
        public async Task<IActionResult> RepairJobs()
        {
            var repairJobs = await _repairJobService.GetAllRepairJobsAsync();
            return View(repairJobs);
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