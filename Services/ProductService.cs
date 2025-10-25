using Google.Cloud.Firestore;
using SupertronicsRepairSystem.Models;
using SupertronicsRepairSystem.ViewModels;
using System.Threading.Tasks;

namespace SupertronicsRepairSystem.Services
{
    public class ProductService : IProductService
    {
        private readonly FirestoreDb _firestoreDb;
        public ProductService(FirestoreDb firestoreDb)
        {
            _firestoreDb = firestoreDb;
        }
        public async Task<bool> AddProductAsync(AddProductViewModel model)
        {
            try
            {
                var product = new Product
                {
                    Name = model.Name,
                    Description = model.Description,
                    ImageUrl = model.ImageUrl,
                    Price = model.Price,
                    WasPrice = model.WasPrice,
                    DiscountPercentage = model.DiscountPercentage
                };
                
                await _firestoreDb.Collection("products").AddAsync(product);
                return true;
            }
            catch
            {
                return false;
            }

        }
    }
}
