using SupertronicsRepairSystem.Models;
using SupertronicsRepairSystem.ViewModels;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SupertronicsRepairSystem.Services
{
    public interface IProductService
    {
       Task<bool> AddProductAsync(AddProductViewModel model);
       Task<List<Product>> GetAllProductsAsync();

        Task<Product?> GetProductByIdAsync(string id);

        Task<bool> UpdateProductAsync(UpdateProductViewModel model);
        Task<bool> DeleteProductAsync(string id);
    }
}
