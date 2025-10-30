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

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> KeepAsideCreate(KeepAsideViewModel model)
        {
            Console.WriteLine("=== KeepAsideCreate POST method called ===");
            Console.WriteLine($"Customer Name: {model.CustomerName}");
            Console.WriteLine($"Customer Surname: {model.CustomerSurname}");
            Console.WriteLine($"Contact Number: {model.ContactNumber}");
            Console.WriteLine($"ID/Passport: {model.IdPassportNumber}");
            Console.WriteLine($"Serial Number: {model.DeviceSerialNumber}");
            Console.WriteLine($"Collection Date: {model.CollectionDate}");
            Console.WriteLine($"ModelState.IsValid: {ModelState.IsValid}");

            // Check if user is logged in
            var userInfo = await _authService.GetCurrentUserInfoAsync();
            if (userInfo == null)
            {
                Console.WriteLine("ERROR: User is not logged in");
                TempData["ErrorMessage"] = "You must be logged in to create a keep aside.";
                return RedirectToAction("SignIn", "Account");
            }

            Console.WriteLine($"User ID: {userInfo.UserId}");
            Console.WriteLine($"User Email: {userInfo.Email}");

            if (!ModelState.IsValid)
            {
                Console.WriteLine("ERROR: Model validation failed");
                foreach (var error in ModelState.Values.SelectMany(v => v.Errors))
                {
                    Console.WriteLine($"Validation error: {error.ErrorMessage}");
                }

                // Reload the product
                try
                {
                    Query query = _firestoreDb.Collection("products").WhereEqualTo("SerialNumber", model.DeviceSerialNumber);
                    QuerySnapshot snapshot = await query.GetSnapshotAsync();
                    var doc = snapshot.Documents.FirstOrDefault();

                    if (doc != null)
                    {
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
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error reloading product: {ex.Message}");
                }

                return View("KeepAsideForm", model);
            }

            try
            {
                var collectionDateUtc = model.CollectionDate.HasValue
                    ? model.CollectionDate.Value.ToUniversalTime()
                    : DateTime.UtcNow.AddDays(2);

                // Create the keep aside document
                var keepAsideData = new Dictionary<string, object>
        {
            { "CustomerId", userInfo.UserId },
            { "CustomerEmail", userInfo.Email },
            { "CustomerName", model.CustomerName ?? "" },
            { "CustomerSurname", model.CustomerSurname ?? "" },
            { "ContactNumber", model.ContactNumber ?? "" },
            { "IdPassportNumber", model.IdPassportNumber ?? "" },
            { "DeviceSerialNumber", model.DeviceSerialNumber ?? "" },
            { "CollectionDate", collectionDateUtc },
            { "CreatedAt", DateTime.UtcNow }
        };

                Console.WriteLine($"Creating keep aside document in Firestore...");

                DocumentReference docRef = await _firestoreDb.Collection("KeepAsides").AddAsync(keepAsideData);

                Console.WriteLine($"SUCCESS: Keep aside created with ID: {docRef.Id}");

                TempData["SuccessMessage"] = "Keep aside created successfully!";
                return RedirectToAction("MyKeepAsides", "Customer");

            }
            catch (Exception ex)
            {
                Console.WriteLine($"ERROR creating keep aside: {ex.Message}");
                Console.WriteLine($"Stack trace: {ex.StackTrace}");
                ModelState.AddModelError(string.Empty, $"An error occurred: {ex.Message}");

                // Reload product on error
                try
                {
                    Query query = _firestoreDb.Collection("products").WhereEqualTo("SerialNumber", model.DeviceSerialNumber);
                    QuerySnapshot snapshot = await query.GetSnapshotAsync();
                    var doc = snapshot.Documents.FirstOrDefault();

                    if (doc != null)
                    {
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
                }
                catch { }

                return View("KeepAsideForm", model);
            }
        }

        // GET: Customer/KeepAside
        public async Task<IActionResult> KeepAside(string serialNumber)
        {
            Console.WriteLine($"=== KeepAside GET method called with serialNumber: {serialNumber} ===");

            // Check if user is logged in
            var userInfo = await _authService.GetCurrentUserInfoAsync();
            if (userInfo == null)
            {
                Console.WriteLine("ERROR: User is not logged in");
                TempData["ErrorMessage"] = "You must be logged in to keep aside a product.";
                return RedirectToAction("SignIn", "Account");
            }

            Console.WriteLine($"User logged in: {userInfo.Email}");

            if (string.IsNullOrEmpty(serialNumber))
            {
                Console.WriteLine("ERROR: Serial number is empty");
                TempData["ErrorMessage"] = "No product selected to keep aside.";
                return RedirectToAction("AllProducts");
            }

            try
            {
                Query query = _firestoreDb.Collection("products").WhereEqualTo("SerialNumber", serialNumber);
                QuerySnapshot snapshot = await query.GetSnapshotAsync();
                var doc = snapshot.Documents.FirstOrDefault();

                if (doc == null)
                {
                    Console.WriteLine($"ERROR: Product with serial {serialNumber} not found");
                    TempData["ErrorMessage"] = "Product not found.";
                    return RedirectToAction("AllProducts");
                }

                var product = doc.ConvertTo<Product>();
                Console.WriteLine($"Product found: {product.Name}");

                // Convert to ProductViewModel
                var productViewModel = new ProductViewModel
                {
                    Id = product.Id,
                    Name = product.Name,
                    SerialNumber = product.SerialNumber,
                    ImageUrl = product.ImageUrl,
                    Price = product.Price
                };

                var model = new KeepAsideViewModel
                {
                    DeviceSerialNumber = product.SerialNumber,
                    CollectionDate = DateTime.Now.AddDays(2),
                    CustomerName = userInfo.FirstName ?? "",
                    CustomerSurname = userInfo.Surname ?? ""
                };

                Console.WriteLine($"Model created with serial: {model.DeviceSerialNumber}");
                Console.WriteLine($"Collection date: {model.CollectionDate}");

                ViewBag.Product = productViewModel;
                return View("KeepAsideForm", model);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"ERROR loading product: {ex.Message}");
                Console.WriteLine($"Stack trace: {ex.StackTrace}");
                TempData["ErrorMessage"] = "Error loading product. Please try again.";
return RedirectToAction("AllProducts", "Customer");
            }
        }

        // GET: Customer/MyKeepAsides
        public async Task<IActionResult> MyKeepAsides()
        {
            Console.WriteLine("=== MyKeepAsides method called ===");

            // Check if user is logged in
            var userInfo = await _authService.GetCurrentUserInfoAsync();
            if (userInfo == null)
            {
                Console.WriteLine("ERROR: User is not logged in");
                TempData["ErrorMessage"] = "You must be logged in to view your keep asides.";
                return RedirectToAction("SignIn", "Account");
            }

            var keepAsides = new List<KeepAsideViewModel>();

            try
            {
                Console.WriteLine($"Loading keep asides for customer: {userInfo.UserId}");

                // FILTER BY CUSTOMER ID
                Query query = _firestoreDb.Collection("KeepAsides")
                    .WhereEqualTo("CustomerId", userInfo.UserId);

                QuerySnapshot snapshot = await query.GetSnapshotAsync();

                Console.WriteLine($"Found {snapshot.Documents.Count} documents");

                foreach (var doc in snapshot.Documents)
                {
                    if (!doc.Exists)
                    {
                        Console.WriteLine($"Document {doc.Id} does not exist");
                        continue;
                    }

                    try
                    {
                        var data = doc.ToDictionary();

                        Console.WriteLine($"Processing document {doc.Id}");

                        // Convert Firestore Timestamp to UTC DateTime
                        DateTime? collectionDate = null;
                        if (data.ContainsKey("CollectionDate"))
                        {
                            if (data["CollectionDate"] is Google.Cloud.Firestore.Timestamp ts)
                            {
                                collectionDate = ts.ToDateTime();
                            }
                        }

                        var keepAside = new KeepAsideViewModel
                        {
                            CustomerName = data.ContainsKey("CustomerName") ? data["CustomerName"]?.ToString() ?? "" : "",
                            CustomerSurname = data.ContainsKey("CustomerSurname") ? data["CustomerSurname"]?.ToString() ?? "" : "",
                            ContactNumber = data.ContainsKey("ContactNumber") ? data["ContactNumber"]?.ToString() ?? "" : "",
                            DeviceSerialNumber = data.ContainsKey("DeviceSerialNumber") ? data["DeviceSerialNumber"]?.ToString() ?? "" : "",
                            CollectionDate = collectionDate,
                            ImageUrl = data.ContainsKey("ImageUrl") ? data["ImageUrl"]?.ToString() : null
                        };

                        keepAsides.Add(keepAside);
                        Console.WriteLine($"Added keep aside for device: {keepAside.DeviceSerialNumber}");
                    }
                    catch (Exception docEx)
                    {
                        Console.WriteLine($"Error processing document {doc.Id}: {docEx.Message}");
                    }
                }

                Console.WriteLine($"Successfully loaded {keepAsides.Count} keep asides for customer {userInfo.UserId}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"ERROR loading keep asides: {ex.Message}");
                Console.WriteLine($"Stack trace: {ex.StackTrace}");
                TempData["Error"] = "Unable to load Keep Asides at this time. Please try again later.";
            }

            return View(keepAsides);
        }
        // GET: Customer/MyKeepAsides - FIXED to filter by customer
     
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