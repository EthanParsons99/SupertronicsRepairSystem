using Google.Cloud.Firestore;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using SupertronicsRepairSystem.Models;
using SupertronicsRepairSystem.ViewModels;
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

        public async Task<IActionResult> RepairJobs(string status, string customer)
        {
            Query query = _repairJobsCollection;

            if (!string.IsNullOrEmpty(status))
            {
                query = query.WhereEqualTo(nameof(RepairJob.Status), status);
            }

            if (!string.IsNullOrEmpty(customer))
            {
                query = query.WhereGreaterThanOrEqualTo(nameof(RepairJob.CustomerName), customer)
                             .WhereLessThanOrEqualTo(nameof(RepairJob.CustomerName), customer + '\uf8ff');
            }

            var snapshot = await query.OrderByDescending(nameof(RepairJob.DateReceived)).GetSnapshotAsync();
            var repairJobs = snapshot.Documents.Select(doc => doc.ConvertTo<RepairJob>()).ToList();

            var viewModels = repairJobs.Select(job => new RepairJobViewModel
            {
                Id = job.Id,
                ItemName = job.ItemModel,
                Status = job.Status,
                CustomerName = job.CustomerName,
                LastUpdated = job.LastUpdated.ToDateTime()
            }).ToList();

            return View(viewModels);
        }

        public IActionResult CreateRepairJob() => View();

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

        public async Task<IActionResult> GenerateRepairQuote(string id)
        {
            var docSnapshot = await _repairJobsCollection.Document(id).GetSnapshotAsync();
            if (!docSnapshot.Exists) return NotFound();
            var repairJob = docSnapshot.ConvertTo<RepairJob>();
            var model = new GenerateRepairQuoteViewModel
            {
                JobId = repairJob.Id,
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
            if (ModelState.IsValid)
            {
                var repairJobRef = _repairJobsCollection.Document(model.JobId);
                var quote = new Quote {  };
                await repairJobRef.UpdateAsync("Quotes", FieldValue.ArrayUnion(quote));
                return RedirectToAction(nameof(RepairJobs));
            }
            return View(model);
        }

        public async Task<IActionResult> UpdateRepairStatus(string id)
        {
            var docSnapshot = await _repairJobsCollection.Document(id).GetSnapshotAsync();
            if (!docSnapshot.Exists) return NotFound();
            var job = docSnapshot.ConvertTo<RepairJob>();

            var model = new UpdateRepairStatusViewModel
            {
                JobId = job.Id,
                ItemModel = job.ItemModel,
                CustomerName = job.CustomerName,
                CurrentStatus = job.Status
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateRepairStatus(UpdateRepairStatusViewModel model)
        {
            if (ModelState.IsValid)
            {
                var jobRef = _repairJobsCollection.Document(model.JobId);
                var updates = new Dictionary<string, object>
                {
                    { nameof(RepairJob.Status), model.NewStatus },
                    { nameof(RepairJob.LastUpdated), Timestamp.FromDateTime(DateTime.UtcNow) }
                };
                await jobRef.UpdateAsync(updates);
                return RedirectToAction(nameof(RepairJobs));
            }
            return View(model);
        }

        public async Task<IActionResult> AddRepairNotes(string id)
        {
            var docSnapshot = await _repairJobsCollection.Document(id).GetSnapshotAsync();
            if (!docSnapshot.Exists) return NotFound();
            var job = docSnapshot.ConvertTo<RepairJob>();

            var model = new AddRepairNotesViewModel
            {
                JobId = job.Id,
                ItemModel = job.ItemModel,
                CustomerName = job.CustomerName,
                ExistingNotes = job.TechnicianNotes.OrderByDescending(n => n.Timestamp).ToList()
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddRepairNotes(AddRepairNotesViewModel model)
        {
            if (ModelState.IsValid)
            {
                var jobRef = _repairJobsCollection.Document(model.JobId);
                var newNote = new Note
                {
                    Content = model.NewNote,
                    Timestamp = Timestamp.FromDateTime(DateTime.UtcNow)
                };

                await jobRef.UpdateAsync(nameof(RepairJob.TechnicianNotes), FieldValue.ArrayUnion(newNote));
                await jobRef.UpdateAsync(nameof(RepairJob.LastUpdated), Timestamp.FromDateTime(DateTime.UtcNow));

                return RedirectToAction(nameof(RepairJobs));
            }

            // If validation fails, reload existing notes before returning the view
            var docSnapshot = await _repairJobsCollection.Document(model.JobId).GetSnapshotAsync();
            if (docSnapshot.Exists)
            {
                model.ExistingNotes = docSnapshot.ConvertTo<RepairJob>().TechnicianNotes.OrderByDescending(n => n.Timestamp).ToList();
            }
            return View(model);
        }

        public IActionResult GenerateProductQuote() => View(); // Placeholder

        public IActionResult TrackSerialNumber() => View(new TrackSerialNumberViewModel());

        [HttpPost]
        public async Task<IActionResult> TrackSerialNumber(TrackSerialNumberViewModel model)
        {
            if (!string.IsNullOrEmpty(model.SerialNumberToSearch))
            {
                var query = _repairJobsCollection.WhereEqualTo(nameof(RepairJob.SerialNumber), model.SerialNumberToSearch);
                var snapshot = await query.GetSnapshotAsync();
                model.FoundJobs = snapshot.Documents.Select(doc => doc.ConvertTo<RepairJob>()).ToList();
            }
            model.SearchPerformed = true;
            return View(model);
        }
    }
}