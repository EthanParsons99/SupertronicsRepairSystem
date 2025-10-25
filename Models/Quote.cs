using Google.Cloud.Firestore;

namespace SupertronicsRepairSystem.Models
{
    [FirestoreData]
    public class Quote
    {
        [FirestoreProperty]
        public string Id { get; set; }

        [FirestoreProperty]
        public double LaborHours { get; set; }

        [FirestoreProperty]
        public double LaborRate { get; set; }

        [FirestoreProperty]
        public double PartsTotal { get; set; }

        [FirestoreProperty]
        public double LaborTotal { get; set; }

        [FirestoreProperty]
        public double SubTotal { get; set; }

        [FirestoreProperty]
        public double TaxAmount { get; set; }

        [FirestoreProperty]
        public double TotalAmount { get; set; }

        [FirestoreProperty]
        public Timestamp DateCreated { get; set; }

        [FirestoreProperty]
        public Timestamp ValidUntil { get; set; }

        [FirestoreProperty]
        public List<QuotePart> QuoteParts { get; set; } = new List<QuotePart>();
    }
}
