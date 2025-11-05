//-----------------------CustomerController --------------------------
using Google.Cloud.Firestore;
using Microsoft.AspNetCore.Mvc;
using SupertronicsRepairSystem.ViewModels;
using SupertronicsRepairSystem.Attributes;
using SupertronicsRepairSystem.Models;
using SupertronicsRepairSystem.Services;
using SupertronicsRepairSystem.ViewModels.Technician;

namespace SupertronicsRepairSystem.Controllers
{
    public class CustomerController : Controller
    {
        private readonly IAuthService _authService;
        private readonly IRepairJobService _repairJobService;
        private readonly FirestoreDb _firestoreDb;

        public CustomerController(IAuthService authService, IRepairJobService repairJobService)
        {
            _authService = authService;
            _repairJobService = repairJobService;

            string path = "path/to/serviceAccountKey.json";
            Environment.SetEnvironmentVariable("GOOGLE_APPLICATION_CREDENTIALS", path);

            _firestoreDb = FirestoreDb.Create("supertronics-dc0f9");
        }

        // ==========================
        // GENERAL / DASHBOARD SECTION
        // ==========================
        public async Task<IActionResult> Index()
        {
            var products = new List<Product>();
            CollectionReference productsRef = _firestoreDb.Collection("products");
            QuerySnapshot snapshot = await productsRef.GetSnapshotAsync();

            foreach (DocumentSnapshot doc in snapshot.Documents)
            {
                if (doc.Exists)
                {
                    products.Add(doc.ConvertTo<Product>());
                }
            }

            return View(products);
        }

        public async Task<IActionResult> Dashboard()
        {
            var userInfo = await _authService.GetCurrentUserInfoAsync();
            ViewBag.UserInfo = userInfo;

            if (userInfo != null)
            {
                var repairJobs = await _repairJobService.GetRepairJobsByCustomerIdAsync(userInfo.UserId);
                ViewBag.RepairJobs = repairJobs;
            }

            return View();
        }

        public async Task<IActionResult> TrackRepair()
        {
            var userInfo = await _authService.GetCurrentUserInfoAsync();
            if (userInfo != null)
            {
                var repairJobs = await _repairJobService.GetRepairJobsByCustomerIdAsync(userInfo.UserId);
                return View(repairJobs);
            }

            return View(new List<RepairJob>());
        }

        // ==========================
        // PRODUCT SECTION
        // ==========================
        public async Task<IActionResult> CustomerViewProduct(string id)
        {
            if (string.IsNullOrEmpty(id)) return RedirectToAction("Index");

            try
            {
                DocumentReference docRef = _firestoreDb.Collection("products").Document(id);
                DocumentSnapshot snapshot = await docRef.GetSnapshotAsync();

                if (!snapshot.Exists)
                {
                    TempData["Error"] = "Product not found.";
                    return RedirectToAction("Index");
                }

                var product = snapshot.ConvertTo<Product>();
                ViewBag.RecommendedProducts = await GetRecommendedProducts(id, 3);

                return View(product);
            }
            catch
            {
                TempData["Error"] = "Error loading product.";
                return RedirectToAction("Index");
            }
        }

        private async Task<List<Product>> GetRecommendedProducts(string excludeId, int count)
        {
            var recommendedProducts = new List<Product>();
            try
            {
                var snapshot = await _firestoreDb.Collection("products").Limit(count + 5).GetSnapshotAsync();

                foreach (var doc in snapshot.Documents)
                {
                    if (doc.Exists && doc.Id != excludeId)
                    {
                        recommendedProducts.Add(doc.ConvertTo<Product>());
                        if (recommendedProducts.Count >= count) break;
                    }
                }
            }
            catch { }

            return recommendedProducts;
        }

        public async Task<IActionResult> AllProducts()
        {
            var products = new List<Product>();
            try
            {
                var snapshot = await _firestoreDb.Collection("products").GetSnapshotAsync();
                foreach (var doc in snapshot.Documents)
                {
                    if (doc.Exists) products.Add(doc.ConvertTo<Product>());
                }
            }
            catch { TempData["Error"] = "Unable to load products at this time."; }

            return View("AllProducts", products);
        }

        // ==========================
        // REPAIR SECTION
        // ==========================
        public IActionResult RequestRepair()
        {
            return View(new CreateRepairJobViewModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RequestRepair(CreateRepairJobViewModel model)
        {
            if (!ModelState.IsValid) return View(model);

            var userInfo = await _authService.GetCurrentUserInfoAsync();
            if (userInfo == null)
            {
                TempData["Error"] = "You must be logged in to request a repair.";
                return RedirectToAction("SignIn", "Account");
            }

            try
            {
                var customerName = $"{userInfo.FirstName} {userInfo.Surname}".Trim();
                if (string.IsNullOrEmpty(customerName)) customerName = userInfo.Email;

                var repairJobId = await _repairJobService.CreateRepairJobAsync(model, userInfo.UserId, customerName);
                TempData["Success"] = $"Repair request submitted! Job ID: {repairJobId[..10]}...";
                return RedirectToAction("TrackRepair");
            }
            catch
            {
                ModelState.AddModelError("", "An error occurred while submitting your repair request.");
                return View(model);
            }
        }

        public async Task<IActionResult> RepairDetails(string id)
        {
            if (string.IsNullOrEmpty(id)) return NotFound();

            var userInfo = await _authService.GetCurrentUserInfoAsync();
            var repairJob = await _repairJobService.GetRepairJobByIdAsync(id);
            if (repairJob == null) return NotFound();

            if (userInfo != null && repairJob.CustomerId != userInfo.UserId) return Forbid();

            return View(repairJob);
        }

        // ==========================
        // QUOTE REQUEST SECTION
        // ==========================

        // GET: Display the quote request form
        public async Task<IActionResult> CustomerGetQuote()
        {
            var model = new CustomerQuoteViewModel
            {
                DeviceTypes = new List<string>
                {
                    "Laptop",
                    "Desktop",
                    "Tablet",
                    "Smartphone",
                    "Printer",
                    "Monitor",
                    "Gaming Console",
                    "Smart TV",
                    "Other"
                }
            };

            // Pre-fill user details if logged in
            var userInfo = await _authService.GetCurrentUserInfoAsync();
            if (userInfo != null)
            {
                model.Name = userInfo.FirstName;
                model.Surname = userInfo.Surname;
                model.Email = userInfo.Email;
            }

            return View("~/Views/Customer/CustomerGetQuote.cshtml", model);
        }

        // POST: Handle quote request submission
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CustomerGetQuote(CustomerQuoteViewModel vm)
        {
            // Debug: Check if method is being called
            System.Diagnostics.Debug.WriteLine("CustomerGetQuote POST method called");

            // Debug: Log all form values
            System.Diagnostics.Debug.WriteLine("=== FORM VALUES ===");
            foreach (var key in Request.Form.Keys)
            {
                System.Diagnostics.Debug.WriteLine($"{key}: {Request.Form[key]}");
            }
            System.Diagnostics.Debug.WriteLine("=== END FORM VALUES ===");

            // Debug: Log model values
            System.Diagnostics.Debug.WriteLine("=== MODEL VALUES ===");
            System.Diagnostics.Debug.WriteLine($"Name: {vm.Name}");
            System.Diagnostics.Debug.WriteLine($"Surname: {vm.Surname}");
            System.Diagnostics.Debug.WriteLine($"Email: {vm.Email}");
            System.Diagnostics.Debug.WriteLine($"PhoneNumber: {vm.PhoneNumber}");
            System.Diagnostics.Debug.WriteLine($"DeviceType: {vm.DeviceType}");
            System.Diagnostics.Debug.WriteLine($"ProblemDescription: {vm.ProblemDescription}");
            System.Diagnostics.Debug.WriteLine("=== END MODEL VALUES ===");

            if (!ModelState.IsValid)
            {
                System.Diagnostics.Debug.WriteLine("Model state is invalid");
                foreach (var error in ModelState.Values.SelectMany(v => v.Errors))
                {
                    System.Diagnostics.Debug.WriteLine($"Validation error: {error.ErrorMessage}");
                }

                // Reload device types if validation fails
                vm.DeviceTypes = new List<string>
                {
                    "Laptop", "Desktop", "Tablet", "Smartphone",
                    "Printer", "Monitor", "Gaming Console", "Smart TV", "Other"
                };
                return View(vm);
            }

            System.Diagnostics.Debug.WriteLine("Model state is valid, attempting to save...");

            try
            {
                var userInfo = await _authService.GetCurrentUserInfoAsync();
                System.Diagnostics.Debug.WriteLine($"User info retrieved: {userInfo?.Email ?? "No user"}");

                // Create quote request data
                var quoteRequestData = new Dictionary<string, object>
                {
                    { "Name", vm.Name ?? "" },
                    { "Surname", vm.Surname ?? "" },
                    { "Email", vm.Email ?? "" },
                    { "PhoneNumber", vm.PhoneNumber ?? "" },
                    { "DeviceType", vm.DeviceType ?? "" },
                    { "Brand", vm.Brand ?? "" },
                    { "Model", vm.Model ?? "" },
                    { "SerialNumber", vm.SerialNumber ?? "" },
                    { "ProblemDescription", vm.ProblemDescription ?? "" },
                    { "Status", "Pending" },
                    { "CreatedAt", Timestamp.FromDateTime(DateTime.UtcNow) },
                    { "CustomerId", userInfo?.UserId ?? "" },
                    { "CustomerName", $"{vm.Name} {vm.Surname}".Trim() }
                };

                System.Diagnostics.Debug.WriteLine("Attempting to save to Firestore...");

                // Save to Firestore as top-level collection
                var docRef = await _firestoreDb.Collection("quoteRequests").AddAsync(quoteRequestData);

                System.Diagnostics.Debug.WriteLine($"Successfully saved with ID: {docRef.Id}");

                TempData["SuccessMessage"] = $"Quote request submitted successfully! Reference: {docRef.Id.Substring(0, 8)}";

                // Redirect based on user authentication
                if (userInfo != null)
                {
                    return RedirectToAction("Dashboard");
                }
                else
                {
                    return RedirectToAction("Index");
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error occurred: {ex.Message}");
                System.Diagnostics.Debug.WriteLine($"Stack trace: {ex.StackTrace}");

                TempData["ErrorMessage"] = $"Error: {ex.Message}";
                ModelState.AddModelError("", "An error occurred while submitting your quote request. Please try again.");
                vm.DeviceTypes = new List<string>
                {
                    "Laptop", "Desktop", "Tablet", "Smartphone",
                    "Printer", "Monitor", "Gaming Console", "Smart TV", "Other"
                };
                return View(vm);
            }
        }

        // ==========================
        // KEEP ASIDE SECTION
        // ==========================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> KeepAsideCreate(KeepAsideViewModel model)
        {
            // Check login
            var userInfo = await _authService.GetCurrentUserInfoAsync();
            if (userInfo == null)
            {
                TempData["ErrorMessage"] = "Login required.";
                return RedirectToAction("SignIn", "Account");
            }

            // Validate model
            if (!ModelState.IsValid)
            {
                await LoadProductForKeepAside(model.DeviceSerialNumber);
                return View("KeepAsideForm", model);
            }

            try
            {
                var keepAsideData = new Dictionary<string, object>
                {
                    { "CustomerId", userInfo.UserId },
                    { "CustomerEmail", userInfo.Email },
                    { "CustomerName", model.CustomerName ?? "" },
                    { "CustomerSurname", model.CustomerSurname ?? "" },
                    { "ContactNumber", model.ContactNumber ?? "" },
                    { "IdPassportNumber", model.IdPassportNumber ?? "" },
                    { "DeviceSerialNumber", model.DeviceSerialNumber ?? "" },
                    { "CollectionDate", model.CollectionDate?.ToUniversalTime() ?? DateTime.UtcNow.AddDays(2) },
                    { "CreatedAt", DateTime.UtcNow }
                };

                await _firestoreDb.Collection("KeepAsides").AddAsync(keepAsideData);
                TempData["SuccessMessage"] = "Keep aside created!";
                return RedirectToAction("MyKeepAsides");
            }
            catch
            {
                ModelState.AddModelError("", "Error creating keep aside.");
                await LoadProductForKeepAside(model.DeviceSerialNumber);
                return View("KeepAsideForm", model);
            }
        }

        public async Task<IActionResult> KeepAside(string serialNumber)
        {
            var userInfo = await _authService.GetCurrentUserInfoAsync();
            if (userInfo == null)
            {
                TempData["ErrorMessage"] = "Login required.";
                return RedirectToAction("SignIn", "Account");
            }

            if (string.IsNullOrEmpty(serialNumber))
            {
                TempData["ErrorMessage"] = "No product selected.";
                return RedirectToAction("AllProducts");
            }

            var model = new KeepAsideViewModel { DeviceSerialNumber = serialNumber, CollectionDate = DateTime.Now.AddDays(2) };
            await LoadProductForKeepAside(serialNumber);
            return View("KeepAsideForm", model);
        }

        public async Task<IActionResult> MyKeepAsides()
        {
            var userInfo = await _authService.GetCurrentUserInfoAsync();
            if (userInfo == null)
            {
                TempData["ErrorMessage"] = "Login required.";
                return RedirectToAction("SignIn", "Account");
            }

            var keepAsides = new List<KeepAsideViewModel>();
            try
            {
                var snapshot = await _firestoreDb.Collection("KeepAsides").WhereEqualTo("CustomerId", userInfo.UserId).GetSnapshotAsync();
                foreach (var doc in snapshot.Documents)
                {
                    if (!doc.Exists) continue;

                    var data = doc.ToDictionary();
                    DateTime? collectionDate = null;
                    if (data.ContainsKey("CollectionDate") && data["CollectionDate"] is Google.Cloud.Firestore.Timestamp ts)
                        collectionDate = ts.ToDateTime();

                    keepAsides.Add(new KeepAsideViewModel
                    {
                        CustomerName = data.GetValueOrDefault("CustomerName")?.ToString(),
                        CustomerSurname = data.GetValueOrDefault("CustomerSurname")?.ToString(),
                        ContactNumber = data.GetValueOrDefault("ContactNumber")?.ToString(),
                        DeviceSerialNumber = data.GetValueOrDefault("DeviceSerialNumber")?.ToString(),
                        CollectionDate = collectionDate,
                        ImageUrl = data.GetValueOrDefault("ImageUrl")?.ToString()
                    });
                }
            }
            catch { TempData["Error"] = "Unable to load Keep Asides at this time."; }

            return View(keepAsides);
        }

        public IActionResult KeepAsideSuccess() => View();

        // ==========================
        // HELPER METHODS
        // ==========================
        private async Task LoadProductForKeepAside(string serialNumber)
        {
            if (string.IsNullOrEmpty(serialNumber)) return;

            try
            {
                var snapshot = await _firestoreDb.Collection("products").WhereEqualTo("SerialNumber", serialNumber).GetSnapshotAsync();
                var doc = snapshot.Documents.FirstOrDefault();
                if (doc == null) return;

                var product = doc.ConvertTo<Product>();
                ViewBag.Product = new ProductViewModel
                {
                    Id = product.Id,
                    Name = product.Name,
                    SerialNumber = product.SerialNumber,
                    ImageUrl = product.ImageUrl,
                    Price = product.Price
                };
            }
            catch { }
        }
    }
}