using Google.Cloud.Firestore;
using SupertronicsRepairSystem.Models;

namespace SupertronicsRepairSystem.Services
{
    public class QuoteService : IQuoteService
    {
        private readonly FirestoreDb _firestoreDb;

        public QuoteService(FirestoreDb firestoreDb)
        {
            _firestoreDb = firestoreDb;
        }

        public async Task<List<RepairJob>> GetAllRepairJobsWithQuotesAsync()
        {
            var repairJobs = new List<RepairJob>();

            var snapshot = await _firestoreDb.Collection("RepairJobs").GetSnapshotAsync();

            foreach (var doc in snapshot.Documents)
            {
                if (!doc.Exists) continue;

                var repairJob = doc.ConvertTo<RepairJob>();
                // Only include repair jobs that have quotes
                if (repairJob.Quotes != null && repairJob.Quotes.Any())
                {
                    repairJobs.Add(repairJob);
                }
            }

            return repairJobs.OrderByDescending(r => r.LastUpdated).ToList();
        }

        public async Task<List<RepairJob>> GetFilteredRepairJobsAsync(
            string status = null,
            string customerId = null,
            DateTime? startDate = null,
            DateTime? endDate = null)
        {
            var snapshot = await _firestoreDb.Collection("RepairJobs").GetSnapshotAsync();

            var repairJobs = new List<RepairJob>();

            foreach (var doc in snapshot.Documents)
            {
                if (!doc.Exists) continue;

                var repairJob = doc.ConvertTo<RepairJob>();

                // Only include jobs with quotes
                if (repairJob.Quotes == null || !repairJob.Quotes.Any())
                    continue;

                // Apply filters
                if (!string.IsNullOrEmpty(status) &&
                    !repairJob.Status.Equals(status, StringComparison.OrdinalIgnoreCase))
                    continue;

                if (!string.IsNullOrEmpty(customerId) &&
                    repairJob.CustomerId != customerId)
                    continue;

                if (startDate.HasValue &&
                    repairJob.LastUpdated.ToDateTime() < startDate.Value)
                    continue;

                if (endDate.HasValue &&
                    repairJob.LastUpdated.ToDateTime() > endDate.Value.AddDays(1))
                    continue;

                repairJobs.Add(repairJob);
            }

            return repairJobs.OrderByDescending(r => r.LastUpdated).ToList();
        }

        public async Task<RepairJob> GetRepairJobByIdAsync(string repairJobId)
        {
            var docRef = _firestoreDb.Collection("RepairJobs").Document(repairJobId);
            var snapshot = await docRef.GetSnapshotAsync();

            if (snapshot.Exists)
            {
                return snapshot.ConvertTo<RepairJob>();
            }

            return null;
        }

        public async Task<bool> UpdateQuoteStatusAsync(string repairJobId, string quoteId, string newStatus)
        {
            try
            {
                var docRef = _firestoreDb.Collection("RepairJobs").Document(repairJobId);
                var snapshot = await docRef.GetSnapshotAsync();

                if (!snapshot.Exists)
                {
                    return false;
                }

                var repairJob = snapshot.ConvertTo<RepairJob>();
                var quote = repairJob.Quotes?.FirstOrDefault(q => q.Id == quoteId);

                if (quote == null)
                {
                    return false;
                }

                // Update the repair job status
                repairJob.Status = newStatus;
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