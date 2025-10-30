using Google.Cloud.Firestore;
using SupertronicsRepairSystem.Models;
using SupertronicsRepairSystem.ViewModels;
using SupertronicsRepairSystem.ViewModels.Technician;

namespace SupertronicsRepairSystem.Services
{
    // Service for managing repair jobs
    public class RepairJobService : IRepairJobService
    {
        private readonly FirestoreDb _firestoreDb;

        public RepairJobService(FirestoreDb firestoreDb)
        {
            _firestoreDb = firestoreDb;
        }

        // Create a new repair job
        public async Task<string> CreateRepairJobAsync(CreateRepairJobViewModel model, string customerId, string customerName)
        {
            try
            {
                // Create a new RepairJob object
                var repairJob = new RepairJob
                {
                    ItemModel = model.ItemModel,
                    SerialNumber = model.SerialNumber,
                    ProblemDescription = model.ProblemDescription,
                    Status = "Pending",
                    DateReceived = Timestamp.FromDateTime(DateTime.UtcNow),
                    LastUpdated = Timestamp.FromDateTime(DateTime.UtcNow),
                    CustomerId = customerId,
                    CustomerName = customerName,
                    Quotes = new List<Quote>(),
                    TechnicianNotes = new List<Note>()
                };

                var docRef = await _firestoreDb.Collection("repairJobs").AddAsync(repairJob);

                // Update the document with its own ID
                await docRef.UpdateAsync("Id", docRef.Id);

                return docRef.Id;
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to create repair job: {ex.Message}");
            }
        }

        // Get a repair job by its ID
        public async Task<RepairJob> GetRepairJobByIdAsync(string repairJobId)
        {
            try
            {
                // Retrieve the repair job document
                var docRef = _firestoreDb.Collection("repairJobs").Document(repairJobId);
                var snapshot = await docRef.GetSnapshotAsync();

                if (snapshot.Exists)
                {
                    return snapshot.ConvertTo<RepairJob>();
                }

                return null;
            }
            catch
            {
                return null;
            }
        }

        // Get all repair jobs for a specific customer
        public async Task<List<RepairJob>> GetRepairJobsByCustomerIdAsync(string customerId)
        {
            try
            {
                var query = _firestoreDb.Collection("repairJobs")
                    .WhereEqualTo("CustomerId", customerId);

                var snapshot = await query.GetSnapshotAsync();
                var repairJobs = new List<RepairJob>();

                foreach (var doc in snapshot.Documents)
                {
                    if (doc.Exists)
                    {
                        repairJobs.Add(doc.ConvertTo<RepairJob>());
                    }
                }

                return repairJobs.OrderByDescending(r => r.DateReceived).ToList();
            }
            catch
            {
                return new List<RepairJob>();
            }
        }

        public async Task<List<RepairJob>> GetAllRepairJobsAsync()
        {
            try
            {
                var snapshot = await _firestoreDb.Collection("repairJobs").GetSnapshotAsync();
                var repairJobs = new List<RepairJob>();

                foreach (var doc in snapshot.Documents)
                {
                    if (doc.Exists)
                    {
                        repairJobs.Add(doc.ConvertTo<RepairJob>());
                    }
                }

                return repairJobs.OrderByDescending(r => r.DateReceived).ToList();
            }
            catch
            {
                return new List<RepairJob>();
            }
        }

        public async Task<bool> UpdateRepairJobStatusAsync(string repairJobId, string newStatus)
        {
            try
            {
                var docRef = _firestoreDb.Collection("repairJobs").Document(repairJobId);

                await docRef.UpdateAsync(new Dictionary<string, object>
                {
                    { "Status", newStatus },
                    { "LastUpdated", Timestamp.FromDateTime(DateTime.UtcNow) }
                });

                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> AddQuoteToRepairJobAsync(string repairJobId, Quote quote)
        {
            try
            {
                var docRef = _firestoreDb.Collection("repairJobs").Document(repairJobId);
                var snapshot = await docRef.GetSnapshotAsync();

                if (!snapshot.Exists)
                {
                    return false;
                }

                var repairJob = snapshot.ConvertTo<RepairJob>();

                // Generate quote ID if not provided
                if (string.IsNullOrEmpty(quote.Id))
                {
                    quote.Id = $"Q{DateTime.UtcNow.Ticks}";
                }

                repairJob.Quotes.Add(quote);
                repairJob.LastUpdated = Timestamp.FromDateTime(DateTime.UtcNow);

                await docRef.SetAsync(repairJob, SetOptions.Overwrite);
                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> AddNoteToRepairJobAsync(string repairJobId, string noteContent)
        {
            try
            {
                var docRef = _firestoreDb.Collection("repairJobs").Document(repairJobId);
                var snapshot = await docRef.GetSnapshotAsync();

                if (!snapshot.Exists)
                {
                    return false;
                }

                var repairJob = snapshot.ConvertTo<RepairJob>();

                var note = new Note
                {
                    Content = noteContent,
                    Timestamp = Timestamp.FromDateTime(DateTime.UtcNow)
                };

                repairJob.TechnicianNotes.Add(note);
                repairJob.LastUpdated = Timestamp.FromDateTime(DateTime.UtcNow);

                await docRef.SetAsync(repairJob, SetOptions.Overwrite);
                return true;
            }
            catch
            {
                return false;
            }
        }
    }
}