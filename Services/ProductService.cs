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
                    DiscountPercentage = model.DiscountPercentage,
                    StockQuantity = model.StockQuantity,
                    SerialNumber = model.SerialNumber
                };
                
                await _firestoreDb.Collection("products").AddAsync(product);
                return true;
            }
            catch
            {
                return false;
            }

        }

        public async Task<List<Product>> GetAllProductsAsync()
        {
            var products = new List<Product>();
            try
            {
                var collectionRef = _firestoreDb.Collection("products");
                var snapshot = await collectionRef.GetSnapshotAsync();

                foreach (var document in snapshot.Documents)
                {
                    var product = document.ConvertTo<Product>();
                    product.Id = document.Id;
                    products.Add(product);
                }
            }
            catch(Exception ex) 
            {
                Console.WriteLine($"Error retrieving products: {ex.Message}");
            }
            return products;
        }
    }
}
