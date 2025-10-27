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

            //  Standard Service Registrations 
            builder.Services.AddControllersWithViews();
            builder.Services.AddHttpContextAccessor();
            builder.Services.AddHttpClient();
            builder.Services.AddScoped<IProductService, ProductService>();
            builder.Services.AddScoped<IQuoteService, QuoteService>();
            builder.Services.AddScoped<IRepairJobService, RepairJobService>();

            //  Firebase Configuration 
            string credentialsPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "google-credentials.json");
            Environment.SetEnvironmentVariable("GOOGLE_APPLICATION_CREDENTIALS", credentialsPath);

            string projectId = builder.Configuration["Firebase:ProjectId"];
            if (string.IsNullOrEmpty(projectId))
            {
                throw new ArgumentNullException(nameof(projectId), "Firebase ProjectId is not set in appsettings.json");
            }

            string firebaseApiKey = builder.Configuration["Firebase:ApiKey"];
            if (string.IsNullOrEmpty(firebaseApiKey))
            {
                throw new ArgumentNullException(nameof(firebaseApiKey), "Firebase ApiKey is not set in appsettings.json");
            }

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
                Console.WriteLine($"FATAL ERROR: Failed to initialize Firebase Admin SDK. Delete function will fail. Error: {ex.Message}");
            }

            // Register Firestore Database as a Singleton
            builder.Services.AddSingleton(new FirestoreDbBuilder
            {
                ProjectId = projectId
            }.Build());

            //  Application Service Registrations 

            // Register IAuthService, passing 5 arguments to the constructor
            builder.Services.AddScoped<IAuthService, FirebaseAuthService>(provider =>
            {
                // Retrieve all required dependencies from the service provider
                var httpContextAccessor = provider.GetRequiredService<IHttpContextAccessor>();
                var firestoreDb = provider.GetRequiredService<FirestoreDb>();
                var firebaseApiKey = builder.Configuration["Firebase:ApiKey"];
                var httpClientFactory = provider.GetRequiredService<IHttpClientFactory>();
                // This is the fifth argument (the new one)
                var firebaseAuth = provider.GetRequiredService<FirebaseAuth>();

                return new FirebaseAuthService(
                    httpContextAccessor,
                    firestoreDb,
                    firebaseApiKey,
                    httpClientFactory,
                    firebaseAuth // <-- The 5th argument is now passed
                );
            });

            builder.Services.AddScoped<IProductService, ProductService>();
            builder.Services.AddScoped<IQuoteService, QuoteService>();
            builder.Services.AddScoped<IRepairJobService, RepairJobService>();

            builder.Services.AddScoped<ITechnicianService, TechnicianService>();


            var app = builder.Build();

            // Configure the HTTP request pipeline.
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
                pattern: "{controller=Customer}/{action=CustomerViewProduct}/{id?}");

            app.Run();
        }
    }
}