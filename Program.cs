using Google.Cloud.Firestore;
using SupertronicsRepairSystem.Services;

namespace SupertronicsRepairSystem
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.
            builder.Services.AddControllersWithViews();
            builder.Services.AddHttpContextAccessor();

            // Add HttpClient factory
            builder.Services.AddHttpClient();

            // Configure Firebase
            var firebaseProjectId = builder.Configuration["Firebase:ProjectId"];
            var firebaseApiKey = builder.Configuration["Firebase:ApiKey"];

            // Initialize Firestore
            var firestoreDb = FirestoreDb.Create(firebaseProjectId);
            builder.Services.AddSingleton(firestoreDb);

            // Register Authentication Service
            builder.Services.AddScoped<IAuthService>(provider =>
            {
                var httpContextAccessor = provider.GetRequiredService<IHttpContextAccessor>();
                var db = provider.GetRequiredService<FirestoreDb>();
                var httpClientFactory = provider.GetRequiredService<IHttpClientFactory>();
                return new FirebaseAuthService(httpContextAccessor, db, firebaseApiKey, httpClientFactory);
            });

            // Add session support
            builder.Services.AddSession(options =>
            {
                options.IdleTimeout = TimeSpan.FromHours(24);
                options.Cookie.HttpOnly = true;
                options.Cookie.IsEssential = true;
            });

            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (!app.Environment.IsDevelopment())
            {
                app.UseExceptionHandler("/Home/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseStaticFiles();
            app.UseRouting();
            app.UseSession();
            app.UseAuthorization();

            app.MapControllerRoute(
                name: "default",
                pattern: "{controller=Home}/{action=Index}/{id?}");

            app.Run();
        }
    }
}