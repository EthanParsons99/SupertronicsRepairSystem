using Google.Cloud.Firestore;

namespace SupertronicsRepairSystem.Models
{
    [FirestoreData]
    public class ProductQuote
    {
        [FirestoreDocumentId]
        public string Id { get; set; }

        [FirestoreProperty]
        public string ProductId { get; set; }

        [FirestoreProperty]
        public string ProductName { get; set; } 

        [FirestoreProperty]
        public int Quantity { get; set; }

        [FirestoreProperty]
        public double UnitPrice { get; set; } 

        [FirestoreProperty]
        public double TotalPrice { get; set; }

        [FirestoreProperty]
        public Timestamp DateCreated { get; set; }
    }
}