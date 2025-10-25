using Google.Cloud.Firestore; 

namespace SupertronicsRepairSystem
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            builder.Services.AddControllersWithViews();

            builder.Services.AddHttpContextAccessor();
            builder.Services.AddHttpClient();

            string credentialsPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "google-credentials.json");
            Environment.SetEnvironmentVariable("GOOGLE_APPLICATION_CREDENTIALS", credentialsPath);

            string projectId = builder.Configuration["Firebase:ProjectId"];
            if (string.IsNullOrEmpty(projectId))
            {
                throw new ArgumentNullException(nameof(projectId), "Firebase ProjectId is not set in appsettings.json");
            }
            builder.Services.AddSingleton(new FirestoreDbBuilder
            {
                ProjectId = projectId
            }.Build());

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
                pattern: "{controller=Technician}/{action=Dashboard}/{id?}");

            app.Run();
        }
    }
}