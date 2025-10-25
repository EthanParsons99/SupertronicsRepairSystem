using System.ComponentModel.DataAnnotations;

namespace SupertronicsRepairSystem.ViewModels.Technician {
    public class CreateRepairJobViewModel {
        [Required]
        public string? CustomerName { get; set; }

        [Required]
        public string? ItemModel { get; set; }
        public string? SerialNumber { get; set; }

        [Required]
        public string? ProblemDescription { get; set; }
    }
}
