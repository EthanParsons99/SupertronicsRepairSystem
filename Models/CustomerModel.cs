using Google.Cloud.Firestore;

namespace SupertronicsRepairSystem.Models
{
    // Represents a product in the Supertronics Repair System
    [FirestoreData]
    public class CustomerModel
    {
        [FirestoreDocumentId]
        public string Id { get; set; }

        [FirestoreProperty]
        public string Name { get; set; }

        [FirestoreProperty]
        public string? Email { get; set; }

        [FirestoreProperty]
        public string? PhoneNumber { get; set; }
    }
}
