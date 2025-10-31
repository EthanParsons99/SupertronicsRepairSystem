using Google.Cloud.Firestore;
using SupertronicsRepairSystem.Models;

namespace SupertronicsRepairSystem.Models
{
    // Represents a note provided for a repair job
    [FirestoreData]
    public class Note
    {
        [FirestoreProperty]
        public string Content { get; set; }

        [FirestoreProperty]
        public Timestamp Timestamp { get; set; }
    }

    // Represents a repair job in the Supertronics Repair System
    [FirestoreData]
    public class RepairJob
    {
        [FirestoreProperty]
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

        [FirestoreProperty]
        public List<Note> TechnicianNotes { get; set; } = new List<Note>();
    }
}