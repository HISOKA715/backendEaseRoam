
using FirebaseAdmin;
using Google.Apis.Auth.OAuth2;
using Google.Cloud.Firestore;
using webCore.Controllers;
using Microsoft.Extensions.DependencyInjection;



var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddHttpClient();
builder.Services.AddScoped<FirebaseNotificationService>();
builder.Services.AddScoped<FirebaseService>();
builder.Services.AddControllersWithViews();

builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromHours(2); // You can set whatever timeout you need
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});
builder.Services.AddHttpContextAccessor();


var firebaseApp = FirebaseApp.Create(new AppOptions()
{
    Credential = GoogleCredential.FromFile("C:/Users/user/Documents/Visual Studio 2022/webCore/Controllers/fir-project-7ba4f-firebase-adminsdk-3pu6p-8d30027601.json"),
});


builder.Services.AddSingleton<FirestoreDb>(provider =>
{
    string filePath = "C:/Users/chunk/Documents/Visual Studio 2022/webCore/Controllers/fir-project-7ba4f-firebase-adminsdk-3pu6p-8d30027601.json";
    Environment.SetEnvironmentVariable("GOOGLE_APPLICATION_CREDENTIALS", filePath);
    return FirestoreDb.Create("fir-project-7ba4f");
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
app.UseSession();
app.UseRouting();

app.UseAuthorization();
app.UseEndpoints(endpoints =>
{
    endpoints.MapControllerRoute(
        name: "default",
        pattern: "{controller=Home}/{action=Index}/{id?}");
});
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
