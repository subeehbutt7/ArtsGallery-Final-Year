using ArtGalleryFinal.Models;
using ArtGalleryFinal.Services;
using ArtGalleryFinal.Data;
using Microsoft.EntityFrameworkCore;
var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services.AddRazorPages(); // Enable Razor Pages
builder.Services.AddControllersWithViews(); // Enable MVC Controllers
builder.Services.AddHttpContextAccessor(); // Register IHttpContextAccessor for accessing session or HTTP context

// Configure session options
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30); // Session timeout duration
    options.Cookie.HttpOnly = true; // Secure the session cookie
    options.Cookie.IsEssential = true; // Mark the session cookie as essential for GDPR compliance
});

// Configure DbContext with a connection string
builder.Services.AddDbContext<ArtGalleryContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("ArtGalleryConnection")));

// Payment (JazzCash, Stripe) and order-confirmation email services
builder.Services.AddScoped<IJazzCashService, JazzCashService>();
builder.Services.AddScoped<IStripePaymentService, StripePaymentService>();
builder.Services.AddScoped<IEmailService, EmailService>();

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<ArtGalleryContext>();
    DbInitializer.Initialize(context);
}
// Configure the HTTP request pipeline
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error"); // Redirect to an error page in production
    app.UseHsts(); // Enforce HSTS for additional security
}

app.UseHttpsRedirection(); // Enforce HTTPS
app.UseStaticFiles(); // Serve static files like CSS, JS, images

app.UseRouting(); // Enable request routing

app.UseSession(); // Use session middleware
app.UseAuthorization(); // Use authorization middleware

// Map Razor Pages and MVC routes
app.MapRazorPages(); // Map Razor Pages

// Add custom routes here


// Default route (keep this last to avoid conflicts)
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}"
);

app.Run(); // Start the application
