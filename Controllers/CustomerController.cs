using Google.Cloud.Firestore;
using Microsoft.AspNetCore.Mvc;
using SupertronicsRepairSystem.Attributes;
using SupertronicsRepairSystem.Models;
using SupertronicsRepairSystem.Services;
using SupertronicsRepairSystem.ViewModels;
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

            return View("AllProducts", products); // Ensure it points to AllProducts.cshtml
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