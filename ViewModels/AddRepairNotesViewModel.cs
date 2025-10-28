using SupertronicsRepairSystem.Data.Models;
using SupertronicsRepairSystem.Models;
using System.ComponentModel.DataAnnotations;

namespace SupertronicsRepairSystem.ViewModels.Technician
{
    public class AddRepairNotesViewModel
    {
        public string JobId { get; set; }
        public string ItemModel { get; set; }
        public string CustomerName { get; set; }

        [Required(ErrorMessage = "Note content cannot be empty.")]
        [StringLength(1000, ErrorMessage = "Note cannot exceed 1000 characters.")]
        [Display(Name = "New Note")]
        public string NewNote { get; set; }

        public List<Note> ExistingNotes { get; set; } = new List<Note>();
    }
}