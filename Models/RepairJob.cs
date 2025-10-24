using Google.Cloud.Firestore;
using SupertronicsRepairSystem.Models;

namespace SupertronicsRepairSystem.Models
{
    [FirestoreData]
    public class RepairJob
    {
        [FirestoreDocumentId]
        public string Id { get; set; }

        [FirestoreProperty]
        public string ItemModel { get; set; }

        [FirestoreProperty]
        public string? SerialNumber { get; set; }

        [FirestoreProperty]
        public string ProblemDescription { get; set; }

        [FirestoreProperty]
        public string Status { get; set; }

        [FirestoreProperty]
        public Timestamp DateReceived { get; set; }

        [FirestoreProperty]
        public Timestamp LastUpdated { get; set; }

        [FirestoreProperty]
        public string CustomerId { get; set; }

        [FirestoreProperty]
        public string CustomerName { get; set; }

        [FirestoreProperty]
        public List<Quote> Quotes { get; set; } = new List<Quote>();
    }
}