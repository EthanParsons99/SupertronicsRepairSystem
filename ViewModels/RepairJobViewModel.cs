namespace SupertronicsRepairSystem.ViewModels.Technician {
    public class RepairJobViewModel {
        public int JobId { get; set; }
        public string? ItemName { get; set; }
        public string? Status { get; set; }
        public string? CustomerName { get; set; }
        public DateTime LastUpdated { get; set; }
        public List<SupertronicsRepairSystem.ViewModels.Technician.QuoteViewModel>? Quotes { get; set; }
    }
}
