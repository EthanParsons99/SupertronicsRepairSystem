using Google.Cloud.Firestore;

namespace SupertronicsRepairSystem.Models
{
    [FirestoreData]
    public class Product
    {
        [FirestoreDocumentId]
        public string Id { get; set; } 

        [FirestoreProperty]
        public string Name { get; set; }

        [FirestoreProperty]
        public string Description { get; set; }

        [FirestoreProperty]
        public string ImageUrl { get; set; }

        [FirestoreProperty]
        public double Price { get; set; } 

        [FirestoreProperty]
        public double WasPrice { get; set; } 

        [FirestoreProperty]
        public int DiscountPercentage { get; set; }

        [FirestoreProperty]
        public int StockQuantity { get; set; }
    }
}