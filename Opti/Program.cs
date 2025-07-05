using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;
using Opti.Data;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Opti.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(connectionString));

Stripe.StripeConfiguration.ApiKey = builder.Configuration["Stripe:sk_test_51RLr7APYeZ7XeGcwEiJpwXP4AA38jmuY4IH2OLnazDAGkHHn5FDosK1T4dhRcvwh8DBJ72QCySdPLtk4egz7yi5Q00a0F7h2A6"];

// Add services to the container.
builder.Services.AddControllersWithViews();

// Add the database context to the services container
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnectionString")));

// Configure Authentication using Cookie Authentication
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Account/Login"; // Redirect to Login if not authenticated
        options.AccessDeniedPath = "/Auth/AccessDenied"; // Custom AccessDenied page
        options.ExpireTimeSpan = TimeSpan.FromMinutes(30); // Session expires after 30 minutes
        options.SlidingExpiration = true; // Sliding expiration (session extends with activity)
    });

// Optional: Configure session management (if you need to store user data across requests)
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30); // Session timeout after 30 minutes of inactivity
    options.Cookie.HttpOnly = true; // Prevent JavaScript access to the session cookie
    options.Cookie.IsEssential = true; // Required for session cookies to function correctly
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts(); // HTTP Strict Transport Security
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

// Enable authentication and authorization middleware
app.UseAuthentication();
app.UseAuthorization();

// Enable session handling middleware (if you use sessions in your app)
app.UseSession();

// Define the default route pattern
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

// Run the application
app.Run();