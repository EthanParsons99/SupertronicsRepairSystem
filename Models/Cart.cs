using Google.Cloud.Firestore;
using System;
using System.Collections.Generic;

namespace SupertronicsRepairSystem.Models
{
    [FirestoreData]
    public class CartItem
    {
        [FirestoreProperty]
        public string ProductId { get; set; }

        [FirestoreProperty]
        public string Name { get; set; }

        [FirestoreProperty]
        public string ImageUrl { get; set; }

        [FirestoreProperty]
        public double UnitPrice { get; set; }

        [FirestoreProperty]
        public int Quantity { get; set; }

        [FirestoreProperty]
        public Timestamp AddedAt { get; set; } = Timestamp.FromDateTime(DateTime.UtcNow);
    }

    [FirestoreData]
    public class Cart
    {
        [FirestoreDocumentId]
        public string Id { get; set; }

        [FirestoreProperty]
        public string UserId { get; set; }

        [FirestoreProperty]
        public string SessionId { get; set; }

        [FirestoreProperty]
        public List<CartItem> Items { get; set; } = new List<CartItem>();

        [FirestoreProperty]
        public Timestamp LastUpdated { get; set; } = Timestamp.FromDateTime(DateTime.UtcNow);
    }
}