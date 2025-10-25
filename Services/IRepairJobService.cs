using SupertronicsRepairSystem.Models;
using SupertronicsRepairSystem.ViewModels;
using SupertronicsRepairSystem.ViewModels.Technician;

namespace SupertronicsRepairSystem.Services
{
    public interface IRepairJobService
    {
        Task<string> CreateRepairJobAsync(CreateRepairJobViewModel model, string customerId, string customerName);
        Task<RepairJob> GetRepairJobByIdAsync(string repairJobId);
        Task<List<RepairJob>> GetRepairJobsByCustomerIdAsync(string customerId);
        Task<List<RepairJob>> GetAllRepairJobsAsync();
        Task<bool> UpdateRepairJobStatusAsync(string repairJobId, string newStatus);
        Task<bool> AddQuoteToRepairJobAsync(string repairJobId, Quote quote);
        Task<bool> AddNoteToRepairJobAsync(string repairJobId, string noteContent);
    }
}