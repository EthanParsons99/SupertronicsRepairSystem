using Google.Cloud.Firestore;

namespace SupertronicsRepairSystem.Models
{
    [FirestoreData]
    public class QuotePart
    {
        [FirestoreProperty]
        public string PartName { get; set; }

        [FirestoreProperty]
        public string? PartNumber { get; set; }

        [FirestoreProperty]
        public string? Description { get; set; }

        [FirestoreProperty]
        public int Quantity { get; set; }

        [FirestoreProperty]
        public double UnitPrice { get; set; }
    }
}
