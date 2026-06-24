using CustomerList.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();

// Register the typed HttpClient for CustomerApiService
builder.Services.AddHttpClient<CustomerApiService>(client =>
{
    client.Timeout = TimeSpan.FromSeconds(15);
});


// In-memory caching, used by CustomerApiService to avoid re-hitting
// the external API on every request.
builder.Services.AddMemoryCache();

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

// Customer is the default controller, so the table appears at the base URL.
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Customer}/{action=Index}/{id?}");

app.Run();
