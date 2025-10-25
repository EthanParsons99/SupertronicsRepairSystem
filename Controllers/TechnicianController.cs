using Google.Cloud.Firestore;
using Microsoft.AspNetCore.Mvc;
using SupertronicsRepairSystem.Models;
using SupertronicsRepairSystem.Models;
using SupertronicsRepairSystem.ViewModels.Technician;
using System.Diagnostics;

namespace SupertronicsRepairSystem.Controllers
{
    public class TechnicianController : Controller
    {
        private readonly FirestoreDb _firestoreDb;
        private readonly CollectionReference _repairJobsCollection;
        private readonly CollectionReference _customersCollection;

        public TechnicianController(FirestoreDb firestoreDb)
        {
            _firestoreDb = firestoreDb;
            _repairJobsCollection = _firestoreDb.Collection("repairJobs");
            _customersCollection = _firestoreDb.Collection("customers");
        }

        public async Task<IActionResult> Dashboard()
        {
            var snapshot = await _repairJobsCollection.GetSnapshotAsync();
            var jobs = snapshot.Documents.Select(doc => doc.ConvertTo<RepairJob>()).ToList();

            var model = new TechnicianDashboardViewModel
            {
                PendingJobs = jobs.Count(j => j.Status == "Pending" || j.Status == "In Progress"),
                CompletedJobs = jobs.Count(j => j.Status == "Completed"),
                QuotesCreated = jobs.Sum(j => j.Quotes?.Count ?? 0)
            };
            return View(model);
        }

        public async Task<IActionResult> RepairJobs()
        {
            var snapshot = await _repairJobsCollection.OrderByDescending("DateReceived").GetSnapshotAsync();
            var repairJobs = snapshot.Documents.Select(doc => doc.ConvertTo<RepairJob>()).ToList();

            var viewModels = repairJobs.Select(job => new RepairJobViewModel
            {
                Id = job.Id, // CHANGED: Was JobId (int), now Id (string)
                ItemName = job.ItemModel,
                Status = job.Status,
                CustomerName = job.CustomerName,
                LastUpdated = job.LastUpdated.ToDateTime()
            }).ToList();

            return View(viewModels);
        }

        public IActionResult CreateRepairJob()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateRepairJob(CreateRepairJobViewModel model)
        {
            if (ModelState.IsValid)
            {
                var customer = new Customer { Name = model.CustomerName };
                var customerDocRef = await _customersCollection.AddAsync(customer);

                var repairJob = new RepairJob
                {
                    ItemModel = model.ItemModel,
                    SerialNumber = model.SerialNumber,
                    ProblemDescription = model.ProblemDescription,
                    CustomerId = customerDocRef.Id,
                    CustomerName = customer.Name,
                    Status = "Pending",
                    DateReceived = Timestamp.FromDateTime(DateTime.UtcNow),
                    LastUpdated = Timestamp.FromDateTime(DateTime.UtcNow)
                };

                await _repairJobsCollection.AddAsync(repairJob);
                return RedirectToAction(nameof(RepairJobs));
            }
            return View(model);
        }

        // CHANGED: Parameter 'id' is now a string
        public async Task<IActionResult> GenerateRepairQuote(string id)
        {
            if (string.IsNullOrEmpty(id)) return BadRequest();

            var docSnapshot = await _repairJobsCollection.Document(id).GetSnapshotAsync();
            if (!docSnapshot.Exists) return NotFound();

            var repairJob = docSnapshot.ConvertTo<RepairJob>();
            var model = new GenerateRepairQuoteViewModel
            {
                JobId = repairJob.Id, // CHANGED: Assign string ID
                CustomerName = repairJob.CustomerName,
                DeviceName = repairJob.ItemModel,
                SerialNumber = repairJob.SerialNumber,
                ProblemDescription = repairJob.ProblemDescription,
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> GenerateRepairQuote(GenerateRepairQuoteViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var repairJobRef = _repairJobsCollection.Document(model.JobId);
            var snapshot = await repairJobRef.GetSnapshotAsync();
            if (!snapshot.Exists) return NotFound();

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

            await repairJobRef.UpdateAsync("Quotes", FieldValue.ArrayUnion(quote));
            await repairJobRef.UpdateAsync("LastUpdated", Timestamp.FromDateTime(DateTime.UtcNow));

            return RedirectToAction(nameof(RepairJobs));
        }

        public IActionResult GenerateProductQuote() => View();

        public IActionResult TrackSerialNumber() => View(new TrackSerialNumberViewModel());

        public IActionResult UpdateRepairStatus(string id)
        {
            ViewData["JobId"] = id; 
            return View();
        }

        public IActionResult AddRepairNotes(string id)
        {
            ViewData["JobId"] = id; 
            return View();
        }
    }
}