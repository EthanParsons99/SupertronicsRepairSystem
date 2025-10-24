using Google.Cloud.Firestore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();

string credentialsPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "google-credentials.json");
Environment.SetEnvironmentVariable("GOOGLE_APPLICATION_CREDENTIALS", credentialsPath);

// Get the Firebase Project ID from appsettings.json
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