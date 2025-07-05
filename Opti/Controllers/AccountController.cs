using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Opti.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using System.Security.Claims;
using System.Threading.Tasks;
using Opti.Data;
using BCrypt.Net;
using Opti.ViewModel; // Ensure you have this namespace for RegisterViewModel
using System.Net.Http;

namespace Opti.Controllers
{
    public class AccountController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<AccountController> _logger;

        public AccountController(ApplicationDbContext context, ILogger<AccountController> logger)
        {
            _context = context;
            _logger = logger;
        }

        // GET: /Account/Login
        public IActionResult Login()
        {
            return View();
        }

        // POST: /Account/Login
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(string username, string password, bool rememberMe)
        {
            if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
            {
                ModelState.AddModelError(string.Empty, "Username and password are required.");
                return View();
            }

            // Find the user in the database
            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.Username == username);

            if (user != null)
            {
                try
                {
                    // Try to verify the password using BCrypt
                    if (BCrypt.Net.BCrypt.Verify(password, user.PasswordHash))
                    {
                        // Create claims for authentication
                        var claims = new List<Claim>
                        {
                            new Claim(ClaimTypes.Name, user.Username),
                            new Claim(ClaimTypes.Email, user.Email),
                            new Claim(ClaimTypes.Role, user.Role),
                            new Claim(ClaimTypes.NameIdentifier, user.UserId.ToString())  // Important: Add this claim for user ID
                        };

                        // Create identity
                        var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
                        var authProperties = new AuthenticationProperties
                        {
                            IsPersistent = rememberMe, // Remember me option
                            ExpiresUtc = DateTime.UtcNow.AddMinutes(30) // Session expiry time
                        };

                        // Sign in the user
                        await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme,
                            new ClaimsPrincipal(claimsIdentity), authProperties);

                        // Redirect based on user role
                        if (user.Role == "Admin")
                        {
                            return RedirectToAction("Index", "AdminDashboard");
                        }
                        else if (user.Role == "Worker")
                        {
                            return RedirectToAction("Index", "WorkerDashboard");
                        }
                        else
                        {
                            return RedirectToAction("Index", "Home"); // Default redirection
                        }
                    }
                    else
                    {
                        ModelState.AddModelError(string.Empty, "Invalid login attempt.");
                        return View();
                    }
                }
                catch (SaltParseException ex)
                {
                    // Handle the SaltParseException and rehash the password
                    _logger.LogWarning(ex, "SaltParseException occurred for user {Username}. Rehashing password.", username);

                    // Rehash the password and save it in the database
                    user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(password);
                    _context.Users.Update(user);
                    await _context.SaveChangesAsync();

                    // Return a message or log the user out and prompt them to login again
                    ModelState.AddModelError(string.Empty, "Your password was rehashed and updated. Please login again.");
                    return View();
                }
            }
            else
            {
                ModelState.AddModelError(string.Empty, "Invalid login attempt.");
                return View();
            }
        }

        // GET: /Account/Register
        public IActionResult Register()
        {
            return View();
        }

        // POST: /Account/Register
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            if (ModelState.IsValid)
            {
                var existingUser = await _context.Users
                    .FirstOrDefaultAsync(u => u.Username == model.Username || u.Email == model.Email);
                if (existingUser != null)
                {
                    ModelState.AddModelError(string.Empty, "Username or Email is already taken.");
                    return View(model);
                }
                var user = new User
                {
                    Username = model.Username,
                    Email = model.Email,
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword(model.Password),
                    Role = "Customer", // Default role is Worker, can be changed to Admin when needed
                    CreatedAt = DateTime.UtcNow,
                    LastLoginDate = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc), // Unix epoch (0 timestamp)
                    LastLogoutDate = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc)  // Unix epoch (0 timestamp)
                };
                try
                {
                    _context.Users.Add(user);
                    int result = await _context.SaveChangesAsync();
                    if (result > 0)
                    {
                        TempData["SuccessMessage"] = "Registration successful! Please log in.";
                        return RedirectToAction("Login");
                    }
                    else
                    {
                        ModelState.AddModelError(string.Empty, "No changes were saved to the database.");
                        return View(model);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error registering user {Username}: {Message}", model.Username, ex.Message);
                    ModelState.AddModelError(string.Empty, $"An error occurred while saving: {ex.Message}");
                    return View(model);
                }
            }
            return View(model);
        }

        // Updated: Log out and redirect to login page - now with cart clearing
        public async Task<IActionResult> Logout()
        {
            // Try to clear the cart before logging out
            if (User.Identity.IsAuthenticated)
            {
                try
                {
                    // Direct database approach instead of HTTP call
                    string userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);

                    if (!string.IsNullOrEmpty(userIdClaim) && int.TryParse(userIdClaim, out int userId))
                    {
                        // Get all orders for this user
                        var userOrders = await _context.CustomerOrders
                            .Where(o => o.UserId == userId)
                            .ToListAsync();

                        // Restore product quantities and remove orders
                        foreach (var order in userOrders)
                        {
                            var product = await _context.Products.FindAsync(order.ProductId);
                            if (product != null)
                            {
                                // Return the quantity back to the product stock
                                product.StockQuantity += order.Quantity;
                                _context.Update(product);
                            }

                            _context.CustomerOrders.Remove(order);
                        }

                        // Save changes to database
                        await _context.SaveChangesAsync();
                        _logger.LogInformation("Cart cleared for user {UserId} during logout", userId);
                    }
                }
                catch (Exception ex)
                {
                    // Log the error but continue with logout
                    _logger.LogError(ex, "Error clearing cart during logout");
                }
            }

            // Original logout code
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("Login");
        }
    }
}