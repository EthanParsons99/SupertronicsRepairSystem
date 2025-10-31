using SupertronicsRepairSystem.Services;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace SupertronicsRepairSystem.ViewModels
{
    // ViewModel for managing technicians
    public class TechnicianManagementViewModel
    {
        [Display(Name = "Search Name/Email")]
        public string? SearchTerm { get; set; }

        public List<UserInfo> Technicians { get; set; } = new List<UserInfo>();
    }
}
