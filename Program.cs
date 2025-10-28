using FirebaseAdmin;
using FirebaseAdmin.Auth;
using Google.Apis.Auth.OAuth2;
using Google.Cloud.Firestore;
using Microsoft.AspNetCore.Http;
using SupertronicsRepairSystem.Services;

namespace SupertronicsRepairSystem
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Standard Service Registrations
            builder.Services.AddControllersWithViews();
            builder.Services.AddHttpContextAccessor();
            builder.Services.AddHttpClient();

            // Firebase Configuration
            string credentialsPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "google-credentials.json");
            Environment.SetEnvironmentVariable("GOOGLE_APPLICATION_CREDENTIALS", credentialsPath);

            string projectId = builder.Configuration["Firebase:ProjectId"];
            string firebaseApiKey = builder.Configuration["Firebase:ApiKey"];

            // Initialize Firebase Admin SDK
            try
            {
                if (FirebaseApp.DefaultInstance == null)
                {
                    FirebaseApp.Create(new AppOptions()
                    {
                        Credential = GoogleCredential.FromFile(credentialsPath),
                    });
                }
                builder.Services.AddSingleton(FirebaseAuth.DefaultInstance);
                Console.WriteLine("Firebase Admin SDK initialized successfully.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"FATAL ERROR: Failed to initialize Firebase Admin SDK. Error: {ex.Message}");
            }

            // Register Firestore Database
            builder.Services.AddSingleton(new FirestoreDbBuilder { ProjectId = projectId }.Build());

            builder.Services.AddScoped<IAuthService>(provider =>
            {
                var httpContextAccessor = provider.GetRequiredService<IHttpContextAccessor>();
                var firestoreDb = provider.GetRequiredService<FirestoreDb>();
                var httpClientFactory = provider.GetRequiredService<IHttpClientFactory>();
                var firebaseAuth = provider.GetRequiredService<FirebaseAuth>();
                return new FirebaseAuthService(httpContextAccessor, firestoreDb, firebaseApiKey, httpClientFactory, firebaseAuth);
            });

            builder.Services.AddScoped<IProductService, ProductService>();
            builder.Services.AddScoped<IQuoteService, QuoteService>();
            builder.Services.AddScoped<IRepairJobService, RepairJobService>();

            var app = builder.Build();

            // Configure the HTTP request pipeline
            if (!app.Environment.IsDevelopment())
            {
                app.UseExceptionHandler("/Home/Error");
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseStaticFiles();
            app.UseRouting();
            app.UseAuthorization();

            app.MapControllerRoute(
                name: "default",
                pattern: "{controller=Customer}/{action=Index}/{id?}");

            app.Run();
        }
    }
}