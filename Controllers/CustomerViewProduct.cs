using Google.Cloud.Firestore;
using Microsoft.AspNetCore.Mvc;
using SupertronicsRepairSystem.Models;
using SupertronicsRepairSystem.Services;

namespace SupertronicsRepairSystem.Controllers
{
    public class CustomerDashboardController : Controller
    {
       
       private readonly FirestoreDb _firestoreDb;
      
          
            string path = "path/to/serviceAccountKey.json";
            Environment.SetEnvironmentVariable("GOOGLE_APPLICATION_CREDENTIALS", path);

            _firestoreDb = FirestoreDb.Create("supertronics-dc0f9");
        }

        public async Task<IActionResult> CustomerViewProduct()
        {
            var products = new List<Product>();

            CollectionReference productsRef = _firestoreDb.Collection("products");
            QuerySnapshot snapshot = await productsRef.GetSnapshotAsync();

            foreach (DocumentSnapshot doc in snapshot.Documents)
            {
                if (doc.Exists)
                {
                    Product product = doc.ConvertTo<Product>();
                    products.Add(product);
                }
            }

            return View(products); // pass the products to the view
        }

    }

}

