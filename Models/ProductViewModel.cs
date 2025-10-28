using Google.Cloud.Firestore;

namespace SupertronicsRepairSystem.Models
{
    [FirestoreData]
    public class ProductViewModel
    {
        [FirestoreProperty]
        public string Name { get; set; }

        [FirestoreProperty]
        public string SerialNumber { get; set; }

        [FirestoreProperty]
        public string ImageUrl { get; set; }
    }

}