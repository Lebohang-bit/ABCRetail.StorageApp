using ABCRetail.StorageApp.Web.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllersWithViews();

builder.Services.AddSingleton<IAzureTableService, AzureTableService>();
builder.Services.AddSingleton<IAzureBlobService, AzureBlobService>();
builder.Services.AddSingleton<IAzureQueueService, AzureQueueService>();
builder.Services.AddSingleton<IAzureFileService, AzureFileService>();

builder.WebHost.UseUrls("http://*:8080");

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseAuthorization();

// ✅ ADD THESE EXPLICIT ROUTES FOR EACH CONTROLLER
app.MapControllerRoute(
    name: "customers",
    pattern: "Customers/{action=Index}/{id?}",
    defaults: new { controller = "Customers", action = "Index" });

app.MapControllerRoute(
    name: "products",
    pattern: "Products/{action=Index}/{id?}",
    defaults: new { controller = "Products", action = "Index" });

app.MapControllerRoute(
    name: "orders",
    pattern: "Orders/{action=Index}/{id?}",
    defaults: new { controller = "Orders", action = "Index" });

app.MapControllerRoute(
    name: "logs",
    pattern: "Logs/{action=Index}/{id?}",
    defaults: new { controller = "Logs", action = "Index" });

// ✅ DEFAULT ROUTE (keep this as fallback)
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Dashboard}/{id?}");

app.Run();