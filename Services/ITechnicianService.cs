using SupertronicsRepairSystem.Data.Models;
using SupertronicsRepairSystem.Models;
using SupertronicsRepairSystem.ViewModels.Technician;

namespace SupertronicsRepairSystem.Services
{
    public interface ITechnicianService
    {
        Task<TechnicianDashboardViewModel> GetDashboardDataAsync();

        Task<List<RepairJob>> GetFilteredRepairJobsAsync(string status, string customer, DateTime? date);

        Task<string> CreateRepairJobAsync(CreateRepairJobViewModel model);
        Task<RepairJob> GetRepairJobByIdAsync(string jobId);
        Task<bool> AddQuoteToRepairJobAsync(string jobId, GenerateRepairQuoteViewModel model);
        Task<bool> UpdateRepairJobStatusAsync(string jobId, string newStatus);
        Task<bool> AddNoteToRepairJobAsync(string jobId, string noteContent);
        Task<List<RepairJob>> FindRepairJobsBySerialNumberAsync(string serialNumber);
        Task<string> CreateProductQuoteAsync(GenerateProductQuoteViewModel model);
    }
}