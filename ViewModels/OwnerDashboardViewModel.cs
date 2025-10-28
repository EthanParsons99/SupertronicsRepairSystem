using SupertronicsRepairSystem.Models;

namespace SupertronicsRepairSystem.ViewModels
{
    public class OwnerDashboardViewModel
    {
        public string OwnerName { get; set; } = "Owner";
        public int TotalProducts { get; set; }
        public int CompletedRepairs { get; set; }
        public int QuotesAccepted { get; set; }
        public int QuotesAssigned { get; set; }
        public List<RecentRepairViewModel> RecentRepairs { get; set; } = new();
        public WarrantyCheckResultViewModel? WarrantyCheckResult { get; set; }
    }

    public class RecentRepairViewModel
    {
        public string Id { get; set; }
        public string CustomerName { get; set; }
        public string? TechnicianName { get; set; }
        public string Status { get; set; }
        public DateTime LastUpdated { get; set; }
    }

    public class WarrantyCheckResultViewModel
    {
        public string SerialNumber { get; set; }
        public string ProductName { get; set; }
        public bool IsValid { get; set; }
    }
}