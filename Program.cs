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
