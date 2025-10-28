using Google.Cloud.Firestore;
using SupertronicsRepairSystem.Data.Models;
using SupertronicsRepairSystem.Models;
using SupertronicsRepairSystem.ViewModels.Technician;

namespace SupertronicsRepairSystem.Services
{
    public class TechnicianService : ITechnicianService
    {
        private readonly FirestoreDb _firestoreDb;
        private readonly CollectionReference _repairJobsCollection;
        private readonly CollectionReference _customersCollection;
        private readonly CollectionReference _productQuotesCollection;

        public TechnicianService(FirestoreDb firestoreDb)
        {
            _firestoreDb = firestoreDb;
            // CORRECTED: Collection name should be consistent (e.g., lowercase camelCase)
            _repairJobsCollection = _firestoreDb.Collection("repairJobs");
            _customersCollection = _firestoreDb.Collection("customers");
            _productQuotesCollection = _firestoreDb.Collection("productQuotes");
        }

        public async Task<TechnicianDashboardViewModel> GetDashboardDataAsync()
        {
            var snapshot = await _repairJobsCollection.GetSnapshotAsync();
            var jobs = snapshot.Documents.Select(doc => doc.ConvertTo<RepairJob>()).ToList();

            return new TechnicianDashboardViewModel
            {
                PendingJobs = jobs.Count(j => j.Status == "Pending" || j.Status == "In Progress" || j.Status == "Diagnosis"),
                CompletedJobs = jobs.Count(j => j.Status == "Completed"),
                QuotesCreated = jobs.Sum(j => j.Quotes?.Count ?? 0)
            };
        }

        public async Task<List<RepairJob>> GetFilteredRepairJobsAsync(string status, string customer, DateTime? date)
        {
            Query query = _repairJobsCollection;

            if (!string.IsNullOrEmpty(status))
            {
                query = query.WhereEqualTo(nameof(RepairJob.Status), status);
            }

            if (!string.IsNullOrEmpty(customer))
            {
                query = query.OrderBy(nameof(RepairJob.CustomerName))
                             .WhereGreaterThanOrEqualTo(nameof(RepairJob.CustomerName), customer)
                             .WhereLessThanOrEqualTo(nameof(RepairJob.CustomerName), customer + '\uf8ff');
            }

            if (date.HasValue)
            {
                var startOfDay = Timestamp.FromDateTime(date.Value.Date.ToUniversalTime());
                var endOfDay = Timestamp.FromDateTime(date.Value.Date.AddDays(1).AddTicks(-1).ToUniversalTime());

                query = query.WhereGreaterThanOrEqualTo(nameof(RepairJob.DateReceived), startOfDay)
                             .WhereLessThanOrEqualTo(nameof(RepairJob.DateReceived), endOfDay);
            }

            query = query.OrderByDescending(nameof(RepairJob.DateReceived));
            var snapshot = await query.GetSnapshotAsync();
            return snapshot.Documents.Select(doc => doc.ConvertTo<RepairJob>()).ToList();
        }

        // THIS IS THE CORRECTED METHOD
        public async Task<string> CreateRepairJobAsync(CreateRepairJobViewModel model)
        {
            // Use the correct Customer model from Data.Models
            var customer = new CustomerModel { Name = model.CustomerName };
            var customerDocRef = await _customersCollection.AddAsync(customer);

            var repairJob = new RepairJob
            {
                // The 'Id' field is intentionally left empty here
                ItemModel = model.ItemModel,
                SerialNumber = model.SerialNumber,
                ProblemDescription = model.ProblemDescription,
                CustomerId = customerDocRef.Id,
                CustomerName = customer.Name,
                Status = "Pending",
                DateReceived = Timestamp.FromDateTime(DateTime.UtcNow),
                LastUpdated = Timestamp.FromDateTime(DateTime.UtcNow)
            };

            // 1. Add the job to Firestore to get its auto-generated document ID
            DocumentReference jobDocRef = await _repairJobsCollection.AddAsync(repairJob);

            // 2. CRITICAL FIX: Update the document we just created with its own ID
            await jobDocRef.UpdateAsync("Id", jobDocRef.Id);

            // 3. Return the new ID so the system knows what it is
            return jobDocRef.Id;
        }

        public async Task<RepairJob> GetRepairJobByIdAsync(string jobId)
        {
            if (string.IsNullOrEmpty(jobId)) return null;
            var docSnapshot = await _repairJobsCollection.Document(jobId).GetSnapshotAsync();
            return docSnapshot.Exists ? docSnapshot.ConvertTo<RepairJob>() : null;
        }

        public async Task<bool> AddQuoteToRepairJobAsync(string jobId, GenerateRepairQuoteViewModel model)
        {
            var repairJobRef = _repairJobsCollection.Document(jobId);
            if (!(await repairJobRef.GetSnapshotAsync()).Exists) return false;

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

            await repairJobRef.UpdateAsync(nameof(RepairJob.Quotes), FieldValue.ArrayUnion(quote));
            await repairJobRef.UpdateAsync(nameof(RepairJob.LastUpdated), Timestamp.FromDateTime(DateTime.UtcNow));
            return true;
        }

        public async Task<bool> UpdateRepairJobStatusAsync(string jobId, string newStatus)
        {
            var jobRef = _repairJobsCollection.Document(jobId);
            if (!(await jobRef.GetSnapshotAsync()).Exists) return false;

            var updates = new Dictionary<string, object>
            {
                { nameof(RepairJob.Status), newStatus },
                { nameof(RepairJob.LastUpdated), Timestamp.FromDateTime(DateTime.UtcNow) }
            };
            await jobRef.UpdateAsync(updates);
            return true;
        }

        public async Task<bool> AddNoteToRepairJobAsync(string jobId, string noteContent)
        {
            var jobRef = _repairJobsCollection.Document(jobId);
            if (!(await jobRef.GetSnapshotAsync()).Exists) return false;

            var newNote = new Note
            {
                Content = noteContent,
                Timestamp = Timestamp.FromDateTime(DateTime.UtcNow)
            };

            await jobRef.UpdateAsync(nameof(RepairJob.TechnicianNotes), FieldValue.ArrayUnion(newNote));
            await jobRef.UpdateAsync(nameof(RepairJob.LastUpdated), Timestamp.FromDateTime(DateTime.UtcNow));
            return true;
        }

        public async Task<List<RepairJob>> FindRepairJobsBySerialNumberAsync(string serialNumber)
        {
            var query = _repairJobsCollection.WhereEqualTo(nameof(RepairJob.SerialNumber), serialNumber);
            var snapshot = await query.GetSnapshotAsync();
            return snapshot.Documents.Select(doc => doc.ConvertTo<RepairJob>()).ToList();
        }

        public async Task<string> CreateProductQuoteAsync(GenerateProductQuoteViewModel model)
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

            var docRef = await _productQuotesCollection.AddAsync(productQuote);
            return productQuote.ProductName;
        }
    }
}