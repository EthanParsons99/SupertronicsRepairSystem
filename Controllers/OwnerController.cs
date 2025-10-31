using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SupertronicsRepairSystem.Attributes;
using SupertronicsRepairSystem.Services;
using SupertronicsRepairSystem.ViewModels;
using System.Threading.Tasks;

namespace SupertronicsRepairSystem.Controllers
{
    // Owner dashboard and management controller
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

        // Owner dashboard with stats
        public async Task<IActionResult> Dashboard()
        {
            var model = new OwnerDashboardViewModel();

            try
            {
                // Get owner name
                var userInfo = await _authService.GetCurrentUserInfoAsync();
                if (userInfo != null)
                {
                    model.OwnerName = !string.IsNullOrEmpty(userInfo.FirstName)
                        ? userInfo.FirstName
                        : userInfo.Email.Split('@')[0];
                }

                // Get total products count
                var products = await _productService.GetAllProductsAsync();
                model.TotalProducts = products?.Count ?? 0;

                // Get all repair jobs for stats
                var allRepairJobs = await _repairJobService.GetAllRepairJobsAsync();

                if (allRepairJobs != null && allRepairJobs.Any())
                {
                    // Count completed repairs
                    model.CompletedRepairs = allRepairJobs.Count(r =>
                        r.Status?.Equals("Completed", StringComparison.OrdinalIgnoreCase) ?? false);

                    // Count repairs with quotes
                    model.QuotesAccepted = allRepairJobs.Count(r =>
                        r.Quotes != null && r.Quotes.Any());

                    // Count assigned repairs
                    model.QuotesAssigned = allRepairJobs.Count(r =>
                        r.Quotes != null &&
                        r.Quotes.Any() &&
                        (r.Status?.Equals("In Progress", StringComparison.OrdinalIgnoreCase) ?? false));

                    // Get recent repairs list
                    model.RecentRepairs = allRepairJobs
                        .OrderByDescending(r => r.LastUpdated.ToDateTime())
                        .Take(10)
                        .Select(r => new RecentRepairViewModel
                        {
                            Id = r.Id,
                            CustomerName = r.CustomerName ?? "Unknown",
                            TechnicianName = GetTechnicianNameFromJob(r),
                            Status = r.Status ?? "Unknown",
                            LastUpdated = r.LastUpdated.ToDateTime()
                        })
                        .ToList();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error loading dashboard: {ex.Message}");
            }

            return View(model);
        }

        // Check warranty by serial number
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CheckWarranty(string serialNumber)
        {
            var model = new OwnerDashboardViewModel();

            try
            {
                // Load dashboard data
                var userInfo = await _authService.GetCurrentUserInfoAsync();
                if (userInfo != null)
                {
                    model.OwnerName = !string.IsNullOrEmpty(userInfo.FirstName)
                        ? userInfo.FirstName
                        : userInfo.Email.Split('@')[0];
                }

                var products = await _productService.GetAllProductsAsync();
                model.TotalProducts = products?.Count ?? 0;

                var allRepairJobs = await _repairJobService.GetAllRepairJobsAsync();

                if (allRepairJobs != null && allRepairJobs.Any())
                {
                    model.CompletedRepairs = allRepairJobs.Count(r =>
                        r.Status?.Equals("Completed", StringComparison.OrdinalIgnoreCase) ?? false);
                    model.QuotesAccepted = allRepairJobs.Count(r =>
                        r.Quotes != null && r.Quotes.Any());
                    model.QuotesAssigned = allRepairJobs.Count(r =>
                        r.Quotes != null &&
                        r.Quotes.Any() &&
                        (r.Status?.Equals("In Progress", StringComparison.OrdinalIgnoreCase) ?? false));

                    model.RecentRepairs = allRepairJobs
                        .OrderByDescending(r => r.LastUpdated.ToDateTime())
                        .Take(10)
                        .Select(r => new RecentRepairViewModel
                        {
                            Id = r.Id,
                            CustomerName = r.CustomerName ?? "Unknown",
                            TechnicianName = GetTechnicianNameFromJob(r),
                            Status = r.Status ?? "Unknown",
                            LastUpdated = r.LastUpdated.ToDateTime()
                        })
                        .ToList();
                }

                // Check warranty
                if (!string.IsNullOrWhiteSpace(serialNumber))
                {
                    var product = products?.FirstOrDefault(p =>
                        p.SerialNumber?.Equals(serialNumber.Trim(), StringComparison.OrdinalIgnoreCase) ?? false);

                    if (product != null)
                    {
                        model.WarrantyCheckResult = new WarrantyCheckResultViewModel
                        {
                            SerialNumber = serialNumber,
                            ProductName = product.Name,
                            IsValid = true
                        };
                    }
                    else
                    {
                        model.WarrantyCheckResult = new WarrantyCheckResultViewModel
                        {
                            SerialNumber = serialNumber,
                            ProductName = "Not Found",
                            IsValid = false
                        };
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error checking warranty: {ex.Message}");
                TempData["Error"] = "An error occurred while checking the warranty.";
            }

            return View("Dashboard", model);
        }

        // Get technician assignment status
        private string GetTechnicianNameFromJob(Models.RepairJob repairJob)
        {
            if (repairJob.Quotes != null && repairJob.Quotes.Any())
            {
                return "Assigned";
            }
            return "Unassigned";
        }

        // Redirect to dashboard
        public IActionResult Index()
        {
            return RedirectToAction("Dashboard");
        }

        // View repair job details
        public async Task<IActionResult> RepairJobDetails(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return RedirectToAction("RepairJobs");
            }

            var repairJob = await _repairJobService.GetRepairJobByIdAsync(id);

            if (repairJob == null)
            {
                TempData["Error"] = "Repair job not found.";
                return RedirectToAction("RepairJobs");
            }

            return View(repairJob);
        }

        // Product management page with filters
        public async Task<IActionResult> ProductManagement(ProductListViewModel filterModel)
        {
            var products = await _productService.GetAllProductsAsync();
            var filteredProducts = products.AsEnumerable();

            // Search filter
            if (!string.IsNullOrWhiteSpace(filterModel.SearchTerm))
            {
                var term = filterModel.SearchTerm.ToLower();
                filteredProducts = filteredProducts.Where(p =>
                p.Name.ToLower().Contains(term) ||
                (p.SerialNumber != null && p.SerialNumber.ToLower().Contains(term))
                );
            }

            // Price filters
            if (filterModel.MinPrice.HasValue)
            {
                filteredProducts = filteredProducts.Where(p => p.Price >= filterModel.MinPrice.Value);
            }
            if (filterModel.MaxPrice.HasValue)
            {
                filteredProducts = filteredProducts.Where(p => p.Price <= filterModel.MaxPrice.Value);
            }
            filterModel.Products = filteredProducts.ToList();

            if (!filterModel.MaxPrice.HasValue || filterModel.MaxPrice.Value <= 0)
            {
                filterModel.MaxPrice = products.Any() ? products.Max(p => p.Price) : 0;
            }
            return View(filterModel);
        }

        // Edit product form
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

        // Update product in DB
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditProduct(UpdateProductViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }
            if (await _productService.UpdateProductAsync(model))
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

        // Delete product from DB
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

        // Add product form
        public IActionResult AddProduct()
        {
            return View(new AddProductViewModel());
        }

        // Save new product to DB
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

        // View all quotes
        public async Task<IActionResult> QuotesManagement()
        {
            var repairJobsWithQuotes = await _quoteService.GetAllRepairJobsWithQuotesAsync();
            return View(repairJobsWithQuotes);
        }

        // Filter quotes by criteria
        [HttpPost]
        public async Task<IActionResult> FilterQuotes(string status, string customerId, DateTime? startDate, DateTime? endDate)
        {
            var filteredJobs = await _quoteService.GetFilteredRepairJobsAsync(status, customerId, startDate, endDate);
            return PartialView("_QuotesTable", filteredJobs);
        }

        // View all repair jobs
        public async Task<IActionResult> RepairJobs()
        {
            var repairJobs = await _repairJobService.GetAllRepairJobsAsync();
            return View(repairJobs);
        }

        // Technician management page with search
        public async Task<IActionResult> TechnicianManagement(TechnicianManagementViewModel filterModel)
        {
            var allTechnicians = await _authService.GetAllTechniciansAsync();
            var filteredTechnicians = allTechnicians.AsEnumerable();

            // Search filter
            if (!string.IsNullOrWhiteSpace(filterModel.SearchTerm))
            {
                var term = filterModel.SearchTerm.Trim().ToLower();
                filteredTechnicians = filteredTechnicians.Where(t =>
                    t.FirstName?.ToLower().Contains(term) == true ||
                    t.Email.ToLower().Contains(term)
                );
            }

            filteredTechnicians = filteredTechnicians.OrderBy(t => t.FirstName);

            filterModel.Technicians = filteredTechnicians.ToList();

            return View(filterModel);
        }

        //Navigate to add technician form
        public IActionResult AddTechnician()
        {
            return View(new AddTechnicianViewModel());
        }

        // Register new technician
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

        // Edit technician form
        public async Task<IActionResult> EditTechnician(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return NotFound();
            }

            var userInfo = await _authService.GetTechnicianByIdAsync(id);
            if (userInfo == null)
            {
                TempData["Error"] = "Technician not found.";
                return RedirectToAction("TechnicianManagement");
            }

            var model = new EditTechnicianViewModel
            {
                UserId = userInfo.UserId,
                FirstName = userInfo.FirstName,
                Surname = userInfo.Surname,
                Email = userInfo.Email,
                PhoneNumber = userInfo.PhoneNumber,
                Role = userInfo.Role
            };

            return View(model);
        }

        // Update technician in DB
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditTechnician(EditTechnicianViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }
            var success = await _authService.UpdateTechnicianAsync(
                model.UserId,
                model
            );
            if (success)
            {
                TempData["Success"] = "Technician updated successfully.";
                return RedirectToAction("TechnicianManagement");
            }
            else
            {
                ModelState.AddModelError(string.Empty, "An error occurred while updating the technician. Please try again.");
                return View(model);
            }
        }

        // Delete technician from DB
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteTechnician(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                TempData["Error"] = "Invalid technician ID.";
                return RedirectToAction("TechnicianManagement");
            }
            var userInfo = await _authService.GetTechnicianByIdAsync(id);
            var technicianName = userInfo != null ? $"{userInfo.FirstName} {userInfo.Surname}".Trim() : "Technician";
            var success = await _authService.DeleteTechnicianAsync(id);

            if (success)
            {
                TempData["Success"] = $"{technicianName} deleted successfully.";
                return RedirectToAction("TechnicianManagement");
            }
            else
            {
                TempData["Error"] = $"An error occurred while deleting {technicianName}. Please try again.";
                return RedirectToAction("TechnicianManagement");
            }
        }
    }
}