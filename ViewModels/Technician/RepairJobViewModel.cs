using SupertronicsRepairSystem.Models;

namespace SupertronicsRepairSystem.ViewModels.Technician
{
    public class RepairJobViewModel
    {
        public string Id { get; set; }
        public string? ItemName { get; set; }
        public string? Status { get; set; }
        public string? SerialNumber { get; set; }
        public string? CustomerName { get; set; }
        public DateTime LastUpdated { get; set; }
        public List<Quote> Quotes { get; set; } = new List<Quote>();
    }
}