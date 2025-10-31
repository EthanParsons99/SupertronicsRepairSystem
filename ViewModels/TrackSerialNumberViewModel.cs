using SupertronicsRepairSystem.Models;
using System.ComponentModel.DataAnnotations;

namespace SupertronicsRepairSystem.ViewModels.Technician
{
    // ViewModel for tracking repair jobs by serial number
    public class TrackSerialNumberViewModel
    {
        [Display(Name = "Serial Number")]
        public string SerialNumberToSearch { get; set; }

        public bool SearchPerformed { get; set; } = false;

        public List<RepairJob> FoundJobs { get; set; } = new List<RepairJob>();
    }
}