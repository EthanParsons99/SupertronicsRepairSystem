using SupertronicsRepairSystem.Models;
using SupertronicsRepairSystem.ViewModels;
using System.Threading.Tasks;

namespace SupertronicsRepairSystem.Services
{
    public interface IProductService
    {
       Task<bool> AddProductAsync(AddProductViewModel model);
    }
}
