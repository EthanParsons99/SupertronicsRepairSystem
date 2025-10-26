using Google.Cloud.Firestore;
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

            // Register Firestore Database as a Singleton
            builder.Services.AddSingleton(new FirestoreDbBuilder
            {
                ProjectId = projectId
            }.Build());

            //  Application Service Registrations 

            builder.Services.AddScoped<IAuthService>(provider =>
                new FirebaseAuthService(
                    provider.GetRequiredService<IHttpContextAccessor>(),
                    provider.GetRequiredService<FirestoreDb>(),
                    firebaseApiKey,
                    provider.GetRequiredService<IHttpClientFactory>()
                )
            );

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