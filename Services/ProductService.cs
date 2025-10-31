using Google.Cloud.Firestore;
using SupertronicsRepairSystem.Models;
using SupertronicsRepairSystem.ViewModels;
using System.Threading.Tasks;

namespace SupertronicsRepairSystem.Services
{
    // Service for managing products in Firestore
    public class ProductService : IProductService
    {
        private readonly FirestoreDb _firestoreDb;
        public ProductService(FirestoreDb firestoreDb)
        {
            _firestoreDb = firestoreDb;
        }
        // Add a new product to Firestore
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

        // Retrieve all products from Firestore
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

        // Retrieve a product by its ID from Firestore
        public async Task<Product?> GetProductByIdAsync(string id)
        {
            try
            {
                var documentRef = _firestoreDb.Collection("products").Document(id);
                var snapshot = await documentRef.GetSnapshotAsync();

                if (!snapshot.Exists)
                {
                    return null;
                }

                var product = snapshot.ConvertTo<Product>();
                product.Id = snapshot.Id;
                return product;
            }
            catch
            {
                return null;
            }
        }

        // Update an existing product in Firestore
        public async Task<bool> UpdateProductAsync(UpdateProductViewModel model)
        {
            try
            {
                var documentRef = _firestoreDb.Collection("products").Document(model.Id);
                var updates = new Dictionary<FieldPath, object>
                {
                    {  new FieldPath("Name"), model.Name },
                    { new FieldPath("Description"), model.Description },
                    { new FieldPath("ImageUrl"), model.ImageUrl },
                    { new FieldPath("Price"), model.Price },
                    { new FieldPath("WasPrice"), model.WasPrice },
                    { new FieldPath("DiscountPercentage"), model.DiscountPercentage },
                    { new FieldPath("StockQuantity"), model.StockQuantity },
                    { new FieldPath("SerialNumber"), model.SerialNumber }
                };
                await documentRef.UpdateAsync(updates);
                return true;
            }
            catch
            {
                return false;
            }
        }

        // Delete a product from Firestore by its ID
        public async Task<bool> DeleteProductAsync(string id)
        {
            try
            {
                var documentRef = _firestoreDb.Collection("products").Document(id);
                await documentRef.DeleteAsync();
                return true;
            }
            catch
            {
                return false;
            }
        }
    }
}
