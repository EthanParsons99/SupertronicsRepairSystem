using SupertronicsRepairSystem.Models;
using System.ComponentModel.DataAnnotations;

namespace SupertronicsRepairSystem.ViewModels.Technician
{
    public class TrackSerialNumberViewModel
    {
        [Display(Name = "Serial Number")]
        public string SerialNumberToSearch { get; set; }

        public bool SearchPerformed { get; set; } = false;

        public List<RepairJob> FoundJobs { get; set; } = new List<RepairJob>();
    }
}