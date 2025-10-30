using Google.Cloud.Firestore;
using SupertronicsRepairSystem.Models;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace SupertronicsRepairSystem.Services
{
    public class CartService : ICartService
    {
        private readonly FirestoreDb _firestoreDb;
        private readonly CollectionReference _productsCollection;
        private readonly CollectionReference _cartsCollection;

        public CartService(FirestoreDb firestoreDb)
        {
            _firestoreDb = firestoreDb;
            _productsCollection = _firestoreDb.Collection("products");
            _cartsCollection = _firestoreDb.Collection("carts");
        }

        public async Task<Cart> GetCartAsync(string userId, string sessionId)
        {
            Query query;
            if (!string.IsNullOrEmpty(userId))
                query = _cartsCollection.WhereEqualTo(nameof(Cart.UserId), userId).Limit(1);
            else
                query = _cartsCollection.WhereEqualTo(nameof(Cart.SessionId), sessionId).Limit(1);

            var snap = await query.GetSnapshotAsync();
            var doc = snap.Documents.FirstOrDefault();
            return doc != null && doc.Exists ? doc.ConvertTo<Cart>() : null;
        }

        public async Task<Cart> CreateCartAsync(string userId, string sessionId)
        {
            var cart = new Cart
            {
                UserId = userId,
                SessionId = sessionId,
                Items = new System.Collections.Generic.List<CartItem>(),
                LastUpdated = Timestamp.FromDateTime(DateTime.UtcNow)
            };

            var docRef = await _cartsCollection.AddAsync(cart);
            // update id field in Firestore document
            await docRef.UpdateAsync(nameof(Cart.Id), docRef.Id);
            cart.Id = docRef.Id;
            return cart;
        }

        private async Task<(Cart cart, DocumentReference docRef)> GetOrCreateCartRefAsync(string userId, string sessionId)
        {
            Query query;
            if (!string.IsNullOrEmpty(userId))
                query = _cartsCollection.WhereEqualTo(nameof(Cart.UserId), userId).Limit(1);
            else
                query = _cartsCollection.WhereEqualTo(nameof(Cart.SessionId), sessionId).Limit(1);

            var snap = await query.GetSnapshotAsync();
            var doc = snap.Documents.FirstOrDefault();
            if (doc != null && doc.Exists)
            {
                return (doc.ConvertTo<Cart>(), doc.Reference);
            }

            var newCart = new Cart
            {
                UserId = userId,
                SessionId = sessionId,
                Items = new System.Collections.Generic.List<CartItem>(),
                LastUpdated = Timestamp.FromDateTime(DateTime.UtcNow)
            };
            var docRef = await _cartsCollection.AddAsync(newCart);
            await docRef.UpdateAsync(nameof(Cart.Id), docRef.Id);
            newCart.Id = docRef.Id;
            return (newCart, docRef);
        }

        public async Task<bool> AddToCartAsync(string userId, string sessionId, string productId, int quantity)
        {
            if (quantity <= 0) return false;

            var productSnap = await _productsCollection.Document(productId).GetSnapshotAsync();
            if (!productSnap.Exists) return false;

            var product = productSnap.ConvertTo<SupertronicsRepairSystem.Models.Product>();

            var (cart, docRef) = await GetOrCreateCartRefAsync(userId, sessionId);

            var existing = cart.Items.FirstOrDefault(i => i.ProductId == productId);
            if (existing != null)
            {
                existing.Quantity += quantity;
            }
            else
            {
                cart.Items.Add(new CartItem
                {
                    ProductId = product.Id,
                    Name = product.Name,
                    ImageUrl = product.ImageUrl,
                    UnitPrice = product.Price,
                    Quantity = quantity,
                    AddedAt = Timestamp.FromDateTime(DateTime.UtcNow)
                });
            }

            cart.LastUpdated = Timestamp.FromDateTime(DateTime.UtcNow);
            await docRef.SetAsync(cart, SetOptions.Overwrite);
            return true;
        }

        public async Task<bool> UpdateQuantityAsync(string userId, string sessionId, string productId, int quantity)
        {
            if (quantity < 0) return false;

            var (cart, docRef) = await GetOrCreateCartRefAsync(userId, sessionId);
            var existing = cart.Items.FirstOrDefault(i => i.ProductId == productId);
            if (existing == null) return false;

            if (quantity == 0)
            {
                cart.Items.Remove(existing);
            }
            else
            {
                existing.Quantity = quantity;
            }

            cart.LastUpdated = Timestamp.FromDateTime(DateTime.UtcNow);
            await docRef.SetAsync(cart, SetOptions.Overwrite);
            return true;
        }

        public async Task<bool> RemoveItemAsync(string userId, string sessionId, string productId)
        {
            var (cart, docRef) = await GetOrCreateCartRefAsync(userId, sessionId);
            var existing = cart.Items.FirstOrDefault(i => i.ProductId == productId);
            if (existing == null) return false;

            cart.Items.Remove(existing);
            cart.LastUpdated = Timestamp.FromDateTime(DateTime.UtcNow);
            await docRef.SetAsync(cart, SetOptions.Overwrite);
            return true;
        }

        public async Task<bool> ClearCartAsync(string userId, string sessionId)
        {
            var (cart, docRef) = await GetOrCreateCartRefAsync(userId, sessionId);
            cart.Items.Clear();
            cart.LastUpdated = Timestamp.FromDateTime(DateTime.UtcNow);
            await docRef.SetAsync(cart, SetOptions.Overwrite);
            return true;
        }
    }
}