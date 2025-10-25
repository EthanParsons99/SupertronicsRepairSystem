using SupertronicsRepairSystem.Models;

namespace SupertronicsRepairSystem.Services
{
    public interface IQuoteService
    {
        Task<List<RepairJob>> GetAllRepairJobsWithQuotesAsync();
        Task<List<RepairJob>> GetFilteredRepairJobsAsync(string status = null, string customerId = null, DateTime? startDate = null, DateTime? endDate = null);
        Task<RepairJob> GetRepairJobByIdAsync(string repairJobId);
        Task<bool> UpdateQuoteStatusAsync(string repairJobId, string quoteId, string newStatus);
    }
}