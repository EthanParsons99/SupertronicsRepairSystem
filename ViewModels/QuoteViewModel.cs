namespace SupertronicsRepairSystem.ViewModels.Technician 
{
    // ViewModel representing a quote for technician use
    public class QuoteViewModel {
        public int QuoteId { get; set; }
        public string? ItemName { get; set; }
        public decimal Price { get; set; }
        public string? Status { get; set; }
        public string? Notes { get; set; }
        public string? Type { get; set; }
        public string? CustomerName { get; set; }
        public int Repairs { get; set; }
        public string? TechnicianName { get; set; }
        public string? Comment { get; set; }
        public int QuoteNumber { get; set; }
    }
}
