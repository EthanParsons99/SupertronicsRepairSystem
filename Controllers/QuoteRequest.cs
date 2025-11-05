using Google.Cloud.Firestore;

namespace SupertronicsRepairSystem.Models
{
    // Represents a quote request submitted by a customer
    [FirestoreData]
    public class QuoteRequest
    {
        [FirestoreDocumentId]
        public string Id { get; set; }

        [FirestoreProperty]
        public string CustomerId { get; set; }

        [FirestoreProperty]
        public string CustomerName { get; set; }

        [FirestoreProperty]
        public string CustomerEmail { get; set; }

        [FirestoreProperty]
        public string? CustomerSurname { get; set; } // Nullable

        [FirestoreProperty]
        public string? PhoneNumber { get; set; }     // Nullable

        [FirestoreProperty]
        public string DeviceType { get; set; }

        [FirestoreProperty]
        public string? Brand { get; set; }           // Nullable

        [FirestoreProperty]
        public string? Model { get; set; }           // Nullable

        [FirestoreProperty]
        public string? SerialNumber { get; set; }    // Nullable

        [FirestoreProperty]
        public string? RepairJobId { get; set; }     // Nullable

        [FirestoreProperty]
        public string ProblemDescription { get; set; }

        [FirestoreProperty]
        public string Status { get; set; } // "Pending", "Quoted", "Rejected"

        [FirestoreProperty]
        public Timestamp DateCreated { get; set; }

        [FirestoreProperty]
        public Timestamp LastUpdated { get; set; }

    }
}