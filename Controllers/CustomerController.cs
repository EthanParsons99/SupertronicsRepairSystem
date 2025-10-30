using Google.Cloud.Firestore;
using Microsoft.AspNetCore.Mvc;
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


        public async Task<IActionResult> Index()
        {
            var products = new List<Product>();

            CollectionReference productsRef = _firestoreDb.Collection("products");
            QuerySnapshot snapshot = await productsRef.GetSnapshotAsync();

            foreach (DocumentSnapshot doc in snapshot.Documents)
            {
                if (doc.Exists)
                {
                    Product product = doc.ConvertTo<Product>();
                    products.Add(product);
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

            return View(new List<SupertronicsRepairSystem.Models.RepairJob>());
        }

        // Controller Method - Fixed to show single product
        public async Task<IActionResult> CustomerViewProduct(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return RedirectToAction("Index"); // Redirect if no ID provided
            }

            try
            {
                // Get single product by ID
                DocumentReference docRef = _firestoreDb.Collection("products").Document(id);
                DocumentSnapshot snapshot = await docRef.GetSnapshotAsync();

                if (snapshot.Exists)
                {
                    Product product = snapshot.ConvertTo<Product>();
                    Console.WriteLine($"Loaded product: {product.Name}");

                    // Get recommended products (3 random products, excluding current one)
                    var recommendedProducts = await GetRecommendedProducts(id, 3);
                    ViewBag.RecommendedProducts = recommendedProducts;

                    return View(product); 
                }
                else
                {
                    TempData["Error"] = "Product not found.";
                    return RedirectToAction("Index");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Firestore error: {ex.Message}");
                TempData["Error"] = "Error loading product.";
                return RedirectToAction("Index");
            }
        }

        private async Task<List<Product>> GetRecommendedProducts(string excludeId, int count)
        {
            var recommendedProducts = new List<Product>();
            try
            {
                CollectionReference productsRef = _firestoreDb.Collection("products");
                QuerySnapshot snapshot = await productsRef.Limit(count + 5).GetSnapshotAsync();

                foreach (DocumentSnapshot doc in snapshot.Documents)
                {
                    if (doc.Exists && doc.Id != excludeId)
                    {
                        Product product = doc.ConvertTo<Product>();
                        recommendedProducts.Add(product);

                        if (recommendedProducts.Count >= count)
                            break;
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error loading recommendations: {ex.Message}");
            }

            return recommendedProducts;
        }


        // GET: Products/All
        // GET: Customer/AllProducts
        public async Task<IActionResult> AllProducts()
        {
            var products = new List<Product>();

            try
            {
                CollectionReference productsRef = _firestoreDb.Collection("products");
                QuerySnapshot snapshot = await productsRef.GetSnapshotAsync();

                foreach (DocumentSnapshot doc in snapshot.Documents)
                {
                    if (doc.Exists)
                    {
                        var product = doc.ConvertTo<Product>();
                        products.Add(product);
                    }
                }

                Console.WriteLine($"Loaded {products.Count} products from Firestore.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error loading products from Firestore: {ex.Message}");
                TempData["Error"] = "Unable to load products at this time. Please try again later.";
            }

            return View("AllProducts", products); 
        }



        // GET: Customer/RequestRepair
        public IActionResult RequestRepair()
        {
            return View(new CreateRepairJobViewModel());
        }

        // POST: Customer/RequestRepair
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RequestRepair(CreateRepairJobViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var userInfo = await _authService.GetCurrentUserInfoAsync();

            if (userInfo == null)
            {
                TempData["Error"] = "You must be logged in to request a repair.";
                return RedirectToAction("SignIn", "Account");
            }

            try
            {
                var customerName = $"{userInfo.FirstName} {userInfo.Surname}".Trim();
                if (string.IsNullOrEmpty(customerName))
                {
                    customerName = userInfo.Email;
                }

                var repairJobId = await _repairJobService.CreateRepairJobAsync(
                    model,
                    userInfo.UserId,
                    customerName
                );

                TempData["Success"] = $"Repair request submitted successfully! Your job ID is: {repairJobId.Substring(0, Math.Min(10, repairJobId.Length))}...";
                return RedirectToAction("TrackRepair");
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, "An error occurred while submitting your repair request. Please try again.");
                return View(model);
            }
        }
        // ===================== Keep Aside Section =====================

        // GET: Customer/KeepAside?s
        public async Task<IActionResult> KeepAside(string serialNumber)
        {
            if (string.IsNullOrEmpty(serialNumber))
            {
                TempData["ErrorMessage"] = "No product selected to keep aside.";
                return RedirectToAction("AllProducts");
            }

            Query query = _firestoreDb.Collection("products").WhereEqualTo("SerialNumber", serialNumber);
            QuerySnapshot snapshot = await query.GetSnapshotAsync();
            var doc = snapshot.Documents.FirstOrDefault();

            if (doc == null)
            {
                TempData["ErrorMessage"] = "Product not found.";
                return RedirectToAction("AllProducts");
            }

            var product = doc.ConvertTo<ProductViewModel>();

            var model = new KeepAsideViewModel
            {
                DeviceSerialNumber = product.SerialNumber,
                CollectionDate = DateTime.Now.AddDays(2) 
            };

            ViewBag.Product = product;
            return View("KeepAsideForm", model); 
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> KeepAsideCreate(KeepAsideViewModel model)
        {
            if (!ModelState.IsValid)
            {
                
                ViewBag.Product = new ProductViewModel
                {
                    SerialNumber = model.DeviceSerialNumber
                };
                return View("KeepAsideForm", model); 
            }
             var collectionDateUtc = model.CollectionDate.HasValue
             ? model.CollectionDate.Value.ToUniversalTime()
             : (DateTime?)null;

            await _firestoreDb.Collection("KeepAsides").Document().SetAsync(new
            {
                model.CustomerName,
                model.CustomerSurname,
                model.ContactNumber,
                model.IdPassportNumber,
                model.DeviceSerialNumber,
                CollectionDate = collectionDateUtc,
                CreatedAt = DateTime.UtcNow
            });

            TempData["SuccessMessage"] = "Keep aside created successfully!";
            return RedirectToAction("MyKeepAsides");
        }


        // GET: Customer/MyKeepAsides
        public async Task<IActionResult> MyKeepAsides()
        {
            var keepAsides = new List<KeepAsideViewModel>();

            try
            {
                var snapshot = await _firestoreDb.Collection("KeepAsides").GetSnapshotAsync();
                foreach (var doc in snapshot.Documents)
                {
                    var data = doc.ToDictionary();

                    // Convert Firestore Timestamp to UTC DateTime if it exists
                    DateTime? collectionDate = null;
                    if (data.ContainsKey("CollectionDate") && data["CollectionDate"] is Google.Cloud.Firestore.Timestamp ts)
                    {
                        collectionDate = ts.ToDateTime();
                    }

                    keepAsides.Add(new KeepAsideViewModel
                    {
                        CustomerName = data.ContainsKey("CustomerName") ? data["CustomerName"].ToString() : "",
                        CustomerSurname = data.ContainsKey("CustomerSurname") ? data["CustomerSurname"].ToString() : "",
                        ContactNumber = data.ContainsKey("ContactNumber") ? data["ContactNumber"].ToString() : "",
                        DeviceSerialNumber = data.ContainsKey("DeviceSerialNumber") ? data["DeviceSerialNumber"].ToString() : "",
                        CollectionDate = collectionDate,
                        ImageUrl = data.ContainsKey("ImageUrl") ? data["ImageUrl"].ToString() : null // optional
                    });
                }
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Unable to load Keep Asides at this time. " + ex.Message;
            }

            return View(keepAsides); // Views/Customer/MyKeepAsides.cshtml
        }

        // GET: Customer/KeepAsideSuccess
        public IActionResult KeepAsideSuccess()
        {
            return View();
        }


        // GET: Customer/RepairDetails/{id}
        public async Task<IActionResult> RepairDetails(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return NotFound();
            }

            var userInfo = await _authService.GetCurrentUserInfoAsync();
            var repairJob = await _repairJobService.GetRepairJobByIdAsync(id);

            if (repairJob == null)
            {
                return NotFound();
            }

            // Ensure customer can only view their own repair jobs
            if (userInfo != null && repairJob.CustomerId != userInfo.UserId)
            {
                return Forbid();
            }

            return View(repairJob);
        }
    }
}