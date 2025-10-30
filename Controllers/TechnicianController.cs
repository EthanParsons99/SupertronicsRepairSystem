using Google.Cloud.Firestore;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.Logging;
using SupertronicsRepairSystem.Models;
using SupertronicsRepairSystem.Services;
using SupertronicsRepairSystem.ViewModels.Technician;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SupertronicsRepairSystem.Controllers
{
    // Technician Controller to manage repair jobs and quotes
    public class TechnicianController : Controller
    {
        private readonly IRepairJobService _repairJobService;
        private readonly FirestoreDb _firestoreDb; 
        private readonly ILogger<TechnicianController> _logger;

        // Constructor
        public TechnicianController(
            IRepairJobService repairJobService,
            FirestoreDb firestoreDb,
            ILogger<TechnicianController> logger)
        {
            _repairJobService = repairJobService;
            _firestoreDb = firestoreDb;
            _logger = logger;
        }

        // POST: Generate Repair Quote
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> GenerateRepairQuote(
            string JobId,
            decimal LaborHours,
            decimal LaborRate,
            List<QuotePartItem> Parts)
        {
            // Validate input
            if (string.IsNullOrEmpty(JobId))
            {
                TempData["ErrorMessage"] = "CRITICAL ERROR: Job ID was lost. Please try again.";
                return RedirectToAction(nameof(RepairJobs));
            }

            // Build the ViewModel
            var model = new GenerateRepairQuoteViewModel
            {
                JobId = JobId,
                LaborHours = LaborHours,
                LaborRate = LaborRate,
                Parts = Parts ?? new List<QuotePartItem>()
            };

            // Create Quote object
            var quote = new Quote
            {
                Id = Guid.NewGuid().ToString(),
                LaborHours = (double)model.LaborHours,
                LaborRate = (double)model.LaborRate,
                PartsTotal = (double)model.PartsTotal,
                LaborTotal = (double)model.LaborTotal,
                SubTotal = (double)model.SubTotal,
                TaxAmount = (double)model.TaxAmount,
                TotalAmount = (double)model.Total,
                ValidUntil = Timestamp.FromDateTime(model.ValidUntil.ToUniversalTime()),
                DateCreated = Timestamp.FromDateTime(DateTime.UtcNow),
                QuoteParts = model.Parts.Select(p => new QuotePart
                {
                    PartName = p.PartName,
                    PartNumber = p.PartNumber,
                    Description = p.Description,
                    Quantity = p.Quantity,
                    UnitPrice = (double)p.UnitPrice
                }).ToList()
            };

            // Add quote to the repair job
            var success = await _repairJobService.AddQuoteToRepairJobAsync(model.JobId, quote);

            if (success)
            {
                TempData["SuccessMessage"] = $"Quote added to job #{model.JobId.Substring(0, 6).ToUpper()}.";
            }
            else
            {
                TempData["ErrorMessage"] = "Error: Could not find the job to add the quote to.";
            }
            return RedirectToAction(nameof(RepairJobs));
        }

        // POST: Generate Product Quote
        // This functiion handles the creation of a product quote
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> GenerateProductQuote(GenerateProductQuoteViewModel model)
        {
            if (ModelState.IsValid)
            {
                var simulatedProductName = $"Product ({model.ProductId})";
                var simulatedUnitPrice = 150.00;
                var totalPrice = simulatedUnitPrice * model.Quantity;
                var productQuote = new ProductQuote
                {
                    ProductId = model.ProductId,
                    ProductName = simulatedProductName,
                    Quantity = model.Quantity,
                    UnitPrice = simulatedUnitPrice,
                    TotalPrice = totalPrice,
                    DateCreated = Timestamp.FromDateTime(DateTime.UtcNow)
                };

                await _firestoreDb.Collection("productQuotes").AddAsync(productQuote);

                TempData["SuccessMessage"] = $"Product quote for '{productQuote.ProductName}' created successfully!";
                return RedirectToAction(nameof(Dashboard));
            }
            return View(model);
        }

        // The main dashboard for technicians

        public async Task<IActionResult> Dashboard()
        {
            var jobs = await _repairJobService.GetAllRepairJobsAsync();
            var model = new TechnicianDashboardViewModel
            {
                PendingJobs = jobs.Count(j => j.Status == "Pending" || j.Status == "In Progress" || j.Status == "Diagnosis"),
                CompletedJobs = jobs.Count(j => j.Status == "Completed"),
                QuotesCreated = jobs.Sum(j => j.Quotes?.Count ?? 0)
            };
            return View(model);
        }

        // View all repair jobs with optional filtering
        public async Task<IActionResult> RepairJobs(string status, string customer, DateTime? date)
        {
            var repairJobs = await _repairJobService.GetAllRepairJobsAsync();

            if (!string.IsNullOrEmpty(status)) { repairJobs = repairJobs.Where(j => j.Status.Equals(status, StringComparison.OrdinalIgnoreCase)).ToList(); }
            if (!string.IsNullOrEmpty(customer)) { repairJobs = repairJobs.Where(j => j.CustomerName.Contains(customer, StringComparison.OrdinalIgnoreCase)).ToList(); }
            if (date.HasValue) { repairJobs = repairJobs.Where(j => j.DateReceived.ToDateTime().Date == date.Value.Date).ToList(); }

            var viewModels = repairJobs.Select(job => new RepairJobViewModel
            {
                Id = job.Id,
                ItemName = job.ItemModel,
                SerialNumber = job.SerialNumber,
                Status = job.Status,
                CustomerName = job.CustomerName,
                LastUpdated = job.LastUpdated.ToDateTime(),
                Quotes = job.Quotes
            }).ToList();

            ViewData["SelectedStatus"] = status;
            ViewData["SelectedCustomer"] = customer;
            ViewData["SelectedDate"] = date?.ToString("yyyy-MM-dd");

            return View(viewModels);
        }

        // Create a new repair job
        public IActionResult CreateRepairJob() => View();

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateRepairJob(CreateRepairJobViewModel model)
        {
            if (ModelState.IsValid)
            {
                await _repairJobService.CreateRepairJobAsync(model, "dummy-customer-id", model.CustomerName);
                TempData["SuccessMessage"] = $"New repair job for '{model.CustomerName}' created successfully.";
                return RedirectToAction(nameof(RepairJobs));
            }
            return View(model);
        }

        // Select a job to generate a quote for
        public async Task<IActionResult> SelectJobForQuote()
        {
            var allJobs = await _repairJobService.GetAllRepairJobsAsync();
            var model = new SelectJobForQuoteViewModel
            {
                OpenJobs = allJobs
                    .Where(j => j.Status != "Completed" && j.Status != "Cancelled")
                    .Select(job => new SelectListItem
                    {
                        Value = job.Id,
                        Text = $"#{job.Id?.Substring(0, 6).ToUpper()} - {job.ItemModel} ({job.CustomerName})"
                    }).ToList()
            };
            return View(model);
        }

        // Generate repair quote for a specific job
        public async Task<IActionResult> GenerateRepairQuote(string id)
        {
            if (string.IsNullOrEmpty(id)) { return RedirectToAction(nameof(SelectJobForQuote)); }
            var repairJob = await _repairJobService.GetRepairJobByIdAsync(id);
            if (repairJob == null) { return NotFound(); }

            var model = new GenerateRepairQuoteViewModel
            {
                JobId = repairJob.Id,
                CustomerName = repairJob.CustomerName,
                DeviceName = repairJob.ItemModel,
                SerialNumber = repairJob.SerialNumber,
                ProblemDescription = repairJob.ProblemDescription
            };
            return View(model);
        }

        // Update repair job status
        public async Task<IActionResult> UpdateRepairStatus(string id)
        {
            var job = await _repairJobService.GetRepairJobByIdAsync(id);

            if (job == null) return NotFound();

            var model = new UpdateRepairStatusViewModel
            {
                JobId = job.Id,
                ItemModel = job.ItemModel,
                CustomerName = job.CustomerName,
                CurrentStatus = job.Status
            };

            return View(model);
        }

        // POST: Update repair job status
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateRepairStatus(UpdateRepairStatusViewModel model)
        {
            if (ModelState.IsValid)
            {
                var success = await _repairJobService.UpdateRepairJobStatusAsync(model.JobId, model.NewStatus);

                if (success) { 
                    TempData["SuccessMessage"] = $"Status for job #{model.JobId.Substring(0, 6).ToUpper()} updated to '{model.NewStatus}'."; 
                }

                else { 
                    TempData["ErrorMessage"] = "Error: Could not find the job to update."; 
                }

                return RedirectToAction(nameof(RepairJobs));
            }
            model.StatusOptions = new List<SelectListItem>
            {
                new SelectListItem { Value = "Pending", Text = "Pending" },
                new SelectListItem { Value = "Diagnosis", Text = "Diagnosis" },
                new SelectListItem { Value = "In Progress", Text = "In Progress" },
                new SelectListItem { Value = "Completed", Text = "Completed" },
                new SelectListItem { Value = "Cancelled", Text = "Cancelled" }
            };

            return View(model);
        }

        // Add repair notes to a job
        public async Task<IActionResult> AddRepairNotes(string id)
        {
            var job = await _repairJobService.GetRepairJobByIdAsync(id);

            if (job == null) return NotFound();

            var model = new AddRepairNotesViewModel
            {
                JobId = job.Id,
                ItemModel = job.ItemModel,
                CustomerName = job.CustomerName,
                ExistingNotes = job.TechnicianNotes.OrderByDescending(n => n.Timestamp).ToList()
            };

            return View(model);
        }

        // POST: Add repair notes to a job
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddRepairNotes(AddRepairNotesViewModel model)
        {
            if (ModelState.IsValid)
            {
                var success = await _repairJobService.AddNoteToRepairJobAsync(model.JobId, model.NewNote);

                if (success) { 
                    TempData["SuccessMessage"] = $"Note added to job #{model.JobId.Substring(0, 6).ToUpper()}."; 
                }
                else { 
                    TempData["ErrorMessage"] = "Error: Could not find the job to add a note to.";
                }

                return RedirectToAction(nameof(RepairJobs));
            }
            var job = await _repairJobService.GetRepairJobByIdAsync(model.JobId);

            if (job != null) { 
                model.ExistingNotes = job.TechnicianNotes.OrderByDescending(n => n.Timestamp).ToList(); 
            }

            return View(model);
        }

        // Track serial number across repair jobs
        public IActionResult TrackSerialNumber() => View(new TrackSerialNumberViewModel());

        // POST: Track serial number across repair jobs
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> TrackSerialNumber(TrackSerialNumberViewModel model)
        {
            if (!string.IsNullOrEmpty(model.SerialNumberToSearch))
            {
                var query = _firestoreDb.Collection("repairJobs").WhereEqualTo("SerialNumber", model.SerialNumberToSearch);
                var snapshot = await query.GetSnapshotAsync();
                model.FoundJobs = snapshot.Documents.Select(doc => doc.ConvertTo<RepairJob>()).ToList();
            }
            model.SearchPerformed = true;
            return View(model);
        }

        public IActionResult GenerateProductQuote() => View(new GenerateProductQuoteViewModel());
    }
}