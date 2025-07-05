using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Opti.Data;
using Opti.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Opti.Controllers
{
    [Authorize(Roles = "Admin")]
    public class UsersController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<UsersController> _logger;

        public UsersController(ApplicationDbContext context, ILogger<UsersController> logger)
        {
            _context = context;
            _logger = logger;
        }

        // GET: /Users/Index
        public async Task<IActionResult> Index()
        {
            try
            {
                var users = await _context.Users
                    .OrderByDescending(u => u.IsActive)
                    .ThenBy(u => u.Username)
                    .ToListAsync();

                return View(users);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving users list");
                TempData["ErrorMessage"] = "An error occurred while loading users.";
                return RedirectToAction("Index", "AdminDashboard");
            }
        }

        // POST: /Users/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(User model, string password)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    // Check if username or email already exists
                    var existingUser = await _context.Users
                        .FirstOrDefaultAsync(u => u.Username == model.Username || u.Email == model.Email);

                    if (existingUser != null)
                    {
                        TempData["ErrorMessage"] = "Username or email is already in use.";
                        return RedirectToAction("Index");
                    }

                    // Create new user
                    var user = new User
                    {
                        Username = model.Username,
                        Email = model.Email,
                        Role = model.Role,
                        PasswordHash = BCrypt.Net.BCrypt.HashPassword(password),
                        IsActive = model.IsActive,
                        CreatedAt = DateTime.UtcNow
                    };

                    _context.Users.Add(user);
                    await _context.SaveChangesAsync();

                    TempData["SuccessMessage"] = "User created successfully.";
                    return RedirectToAction("Index");
                }

                TempData["ErrorMessage"] = "Invalid user data. Please check the form.";
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating user");
                TempData["ErrorMessage"] = "An error occurred while creating the user.";
                return RedirectToAction("Index");
            }
        }

        // POST: /Users/Edit
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, User model, string password)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    var user = await _context.Users.FindAsync(id);
                    if (user == null)
                    {
                        TempData["ErrorMessage"] = "User not found.";
                        return RedirectToAction("Index");
                    }

                    // Check if username or email already exists for another user
                    var existingUser = await _context.Users
                        .FirstOrDefaultAsync(u => (u.Username == model.Username || u.Email == model.Email) && u.UserId != id);

                    if (existingUser != null)
                    {
                        TempData["ErrorMessage"] = "Username or email is already in use by another user.";
                        return RedirectToAction("Index");
                    }

                    // Update user properties
                    user.Username = model.Username;
                    user.Email = model.Email;
                    user.Role = model.Role;
                    user.IsActive = model.IsActive;

                    // Update password if provided
                    if (!string.IsNullOrEmpty(password))
                    {
                        user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(password);
                    }

                    _context.Update(user);
                    await _context.SaveChangesAsync();

                    TempData["SuccessMessage"] = "User updated successfully.";
                    return RedirectToAction("Index");
                }

                TempData["ErrorMessage"] = "Invalid user data. Please check the form.";
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating user {UserId}", id);
                TempData["ErrorMessage"] = "An error occurred while updating the user.";
                return RedirectToAction("Index");
            }
        }

        // POST: /Users/Delete
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                var user = await _context.Users.FindAsync(id);
                if (user == null)
                {
                    TempData["ErrorMessage"] = "User not found.";
                    return RedirectToAction("Index");
                }

                // Check if user has associated data
                bool hasOrders = await _context.CustomerOrders.AnyAsync(o => o.UserId == id);
                bool hasProductionOrders = await _context.ProductionOrders.AnyAsync(o => o.UserId == id);

                if (hasOrders || hasProductionOrders)
                {
                    // Instead of deleting, deactivate the user to preserve referential integrity
                    user.IsActive = false;
                    _context.Update(user);
                    await _context.SaveChangesAsync();

                    TempData["WarningMessage"] = "User has associated data and was deactivated instead of deleted.";
                    return RedirectToAction("Index");
                }

                // If no associated data, proceed with deletion
                _context.Users.Remove(user);
                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = "User deleted successfully.";
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting user {UserId}", id);
                TempData["ErrorMessage"] = "An error occurred while deleting the user.";
                return RedirectToAction("Index");
            }
        }

        // GET: /Users/Details/5 (AJAX)
        [HttpGet]
        public async Task<IActionResult> Details(int id)
        {
            try
            {
                var user = await _context.Users.FindAsync(id);
                if (user == null)
                {
                    return NotFound();
                }

                // Get user stats
                int orderCount = await _context.CustomerOrders.CountAsync(o => o.UserId == id);

                // Calculate days active
                int daysActive = 0;
                if (user.LastLoginDate.HasValue)
                {
                    daysActive = (DateTime.Now - user.LastLoginDate.Value).Days;
                }
                else
                {
                    daysActive = (DateTime.Now - user.CreatedAt).Days;
                }

                // Count logins (this would be better if implemented with a separate LoginHistory table)
                int loginCount = 0; // Placeholder value

                // In a real app with login history, you would do something like:
                // int loginCount = await _context.LoginHistory.CountAsync(l => l.UserId == id);

                return Json(new
                {
                    user.UserId,
                    user.Username,
                    user.Email,
                    user.Role,
                    user.IsActive,
                    LastLogin = user.LastLoginDate?.ToString("yyyy-MM-dd HH:mm") ?? "Never",
                    OrderCount = orderCount,
                    DaysActive = daysActive,
                    LoginCount = loginCount
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving user details for user {UserId}", id);
                return StatusCode(500, "An error occurred while retrieving user details.");
            }
        }

        // For AdminDashboard integration
        [Route("AdminDashboard/Users")]
        public async Task<IActionResult> AdminDashboardUsers()
        {
            return await Index();
        }
    }
}