using Google.Cloud.Firestore;
using SupertronicsRepairSystem.Services;

namespace SupertronicsRepairSystem.Models
{
    [FirestoreData]
    public class TechnicianModel
    {
        [FirestoreDocumentId]
        public string Id { get; set; }

        [FirestoreProperty]
        public string FirstName { get; set; }
        [FirestoreProperty]
        public string Surname { get; set; }
        [FirestoreProperty]
        public string Email { get; set; }
        [FirestoreProperty]
        public string PhoneNumber { get; set; }

        [FirestoreProperty]
        public UserRole Role { get; set; } = UserRole.Technician;

        [FirestoreProperty]
        public Timestamp CreatedAt { get; set; }
    }
}
