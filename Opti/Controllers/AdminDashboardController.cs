using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Opti.Data;
using Opti.Models;
using BCrypt.Net;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Opti.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminDashboardController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<AdminDashboardController> _logger;

        public AdminDashboardController(ApplicationDbContext context, ILogger<AdminDashboardController> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<IActionResult> Index()
        {
            try
            {
                // Log user info for debugging
                var username = User.Identity?.Name;
                var isAuthenticated = User.Identity?.IsAuthenticated ?? false;
                var userRoles = string.Join(", ", User.Claims
                    .Where(c => c.Type == System.Security.Claims.ClaimTypes.Role)
                    .Select(c => c.Value));

                _logger.LogInformation(
                    "AdminDashboard accessed by {Username}, Authenticated: {IsAuthenticated}, Roles: {Roles}",
                    username, isAuthenticated, userRoles);

                // Calculate total revenue from customer orders
                decimal totalRevenue = await _context.CustomerOrders.SumAsync(o => o.TotalAmount);

                // Get user statistics
                int newCustomers = await _context.Users
                    .Where(u => u.CreatedAt >= DateTime.Now.AddDays(-30))
                    .CountAsync();

                int activeAccounts = await _context.Users.CountAsync();

                // Calculate growth rate
                double growthRate = activeAccounts > 0 ? (double)newCustomers / activeAccounts * 100 : 0;

                // Get machine statistics
                int operationalMachines = await _context.Machines
                    .CountAsync(m => m.Status == "Operational");

                int machinesUnderMaintenance = await _context.Machines
                    .CountAsync(m => m.Status == "Under Maintenance");

                int offlineMachines = await _context.Machines
                    .CountAsync(m => m.Status == "Offline");

                // Get order statistics
                int pendingOrders = await _context.ProductionOrders
                    .CountAsync(o => o.Status == "Pending");

                int completedOrders = await _context.ProductionOrders
                    .CountAsync(o => o.Status == "Completed");

                int inProgressOrders = await _context.ProductionOrders
                    .CountAsync(o => o.Status == "In Progress");

                // Get recent orders with product and user details
                var recentOrders = await _context.CustomerOrders
                    .Include(o => o.Product)
                    .Include(o => o.User)
                    .OrderByDescending(o => o.OrderDate)
                    .Take(5)
                    .ToListAsync();

                // Get monthly sales data (last 7 months)
                decimal[] monthlySales = await GetMonthlySalesData();

                // Get monthly new users (last 7 months)
                int[] monthlyCustomerGrowth = await GetMonthlyCustomerGrowthData();

                // Get machine type breakdown
                var machineTypeSummary = await GetMachineTypeSummary();

                var viewModel = new AdminDashboardViewModel
                {
                    TotalRevenue = totalRevenue,
                    NewCustomers = newCustomers,
                    ActiveAccounts = activeAccounts,
                    GrowthRate = growthRate,
                    OperationalMachines = operationalMachines,
                    MachinesUnderMaintenance = machinesUnderMaintenance,
                    OfflineMachines = offlineMachines,
                    PendingOrders = pendingOrders,
                    CompletedOrders = completedOrders,
                    InProgressOrders = inProgressOrders,
                    RecentOrders = recentOrders,
                    MonthlySales = monthlySales,
                    MonthlyCustomerGrowth = monthlyCustomerGrowth,
                    MachineTypeSummary = machineTypeSummary
                };

                return View(viewModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading admin dashboard: {Message}", ex.Message);
                TempData["ErrorMessage"] = "An error occurred while loading the dashboard.";
                return RedirectToAction("Error", "Home");
            }
        }

        private async Task<decimal[]> GetMonthlySalesData()
        {
            var today = DateTime.Today;
            var result = new decimal[7];

            for (int i = 6; i >= 0; i--)
            {
                var startDate = new DateTime(today.Year, today.Month, 1).AddMonths(-i);
                var endDate = startDate.AddMonths(1).AddDays(-1);

                result[6 - i] = await _context.CustomerOrders
                    .Where(o => o.OrderDate >= startDate && o.OrderDate <= endDate)
                    .SumAsync(o => o.TotalAmount);
            }

            return result;
        }

        private async Task<int[]> GetMonthlyCustomerGrowthData()
        {
            var today = DateTime.Today;
            var result = new int[7];

            for (int i = 6; i >= 0; i--)
            {
                var startDate = new DateTime(today.Year, today.Month, 1).AddMonths(-i);
                var endDate = startDate.AddMonths(1).AddDays(-1);

                result[6 - i] = await _context.Users
                    .Where(u => u.CreatedAt >= startDate && u.CreatedAt <= endDate)
                    .CountAsync();
            }

            return result;
        }

        private async Task<List<MachineTypeSummary>> GetMachineTypeSummary()
        {
            var machineGroups = await _context.Machines
                .GroupBy(m => m.MachineType)
                .Select(g => new MachineTypeSummary
                {
                    MachineType = g.Key,
                    Total = g.Count(),
                    Operational = g.Count(m => m.Status == "Operational"),
                    UnderMaintenance = g.Count(m => m.Status == "Under Maintenance"),
                    Offline = g.Count(m => m.Status == "Offline")
                })
                .ToListAsync();

            return machineGroups;
        }

        // User Management Methods

        [HttpGet]
        public IActionResult UserManagement()
        {
            return View();
        }
        [HttpGet]
        public IActionResult AdminMachine()
        {
            return View();
        }

        // Keep existing AdminUser method for backward compatibility
        public IActionResult AdminUser()
        {
            return RedirectToAction("UserManagement");
        }

        [HttpGet]
        public async Task<IActionResult> GetUsers()
        {
            try
            {
                var users = await _context.Users
                    .Select(u => new
                    {
                        u.UserId,
                        u.Username,
                        u.Email,
                        u.Role,
                        u.IsActive,
                        u.CreatedAt,
                        u.LastLoginDate
                    })
                    .OrderByDescending(u => u.CreatedAt)
                    .ToListAsync();

                return Json(users);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching users");
                return Json(new { error = "Failed to load users" });
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetUserStats()
        {
            try
            {
                // Total users
                int totalUsers = await _context.Users.CountAsync();

                // Admin users
                int adminUsers = await _context.Users
                    .CountAsync(u => u.Role == "Admin");

                // Worker users
                int workerUsers = await _context.Users
                    .CountAsync(u => u.Role == "Worker");

                // Customer users
                int customerUsers = await _context.Users
                    .CountAsync(u => u.Role == "Customer");

                // New users in the last 30 days
                DateTime thirtyDaysAgo = DateTime.Now.AddDays(-30);
                int newUsers = await _context.Users
                    .CountAsync(u => u.CreatedAt >= thirtyDaysAgo);

                // Growth rate calculation
                int usersPrevious30Days = await _context.Users
                    .CountAsync(u => u.CreatedAt >= thirtyDaysAgo.AddDays(-30) && u.CreatedAt < thirtyDaysAgo);

                double growthRate = 0;
                if (usersPrevious30Days > 0)
                {
                    growthRate = ((double)newUsers - usersPrevious30Days) / usersPrevious30Days * 100;
                }

                return Json(new
                {
                    totalUsers,
                    adminUsers,
                    workerUsers,
                    customerUsers,
                    newUsers,
                    growthRate = Math.Round(growthRate, 1)
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error calculating user stats");
                return Json(new { error = "Failed to load user statistics" });
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetUser(int id)
        {
            try
            {
                var user = await _context.Users
                    .Where(u => u.UserId == id)
                    .Select(u => new
                    {
                        u.UserId,
                        u.Username,
                        u.Email,
                        u.Role,
                        u.IsActive,
                        u.CreatedAt,
                        u.LastLoginDate
                    })
                    .FirstOrDefaultAsync();

                if (user == null)
                {
                    return Json(new { success = false, message = "User not found." });
                }

                return Json(new { success = true, user });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching user details");
                return Json(new { success = false, message = $"An error occurred: {ex.Message}" });
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateUser([FromBody] UserCreateModel model)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return Json(new { success = false, message = "Invalid user data." });
                }

                // Check if username or email already exists
                var existingUser = await _context.Users
                    .FirstOrDefaultAsync(u => u.Username == model.Username || u.Email == model.Email);

                if (existingUser != null)
                {
                    return Json(new
                    {
                        success = false,
                        message = existingUser.Username == model.Username
                            ? "Username is already taken."
                            : "Email is already in use."
                    });
                }

                // Create new user
                var user = new User
                {
                    Username = model.Username,
                    Email = model.Email,
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword(model.Password),
                    Role = model.Role,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow
                };

                _context.Users.Add(user);
                await _context.SaveChangesAsync();

                _logger.LogInformation("New user created: {Username} with role {Role}", user.Username, user.Role);

                return Json(new { success = true, message = "User created successfully." });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating user");
                return Json(new { success = false, message = $"An error occurred: {ex.Message}" });
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateUser([FromBody] UserUpdateModel model)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return Json(new { success = false, message = "Invalid user data." });
                }

                // Find the user
                var user = await _context.Users.FindAsync(model.UserId);
                if (user == null)
                {
                    return Json(new { success = false, message = "User not found." });
                }

                // Check if username/email is already taken by another user
                var existingUser = await _context.Users
                    .FirstOrDefaultAsync(u =>
                        u.UserId != model.UserId &&
                        (u.Username == model.Username || u.Email == model.Email));

                if (existingUser != null)
                {
                    return Json(new
                    {
                        success = false,
                        message = existingUser.Username == model.Username
                            ? "Username is already taken."
                            : "Email is already in use."
                    });
                }

                // Update user details
                user.Username = model.Username;
                user.Email = model.Email;
                user.Role = model.Role;
                user.IsActive = model.Status == "Active";

                // Update password if requested
                if (model.ResetPassword && !string.IsNullOrEmpty(model.NewPassword))
                {
                    user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(model.NewPassword);
                }

                _context.Users.Update(user);
                await _context.SaveChangesAsync();

                _logger.LogInformation("User updated: {Username}", user.Username);

                return Json(new { success = true, message = "User updated successfully." });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating user");
                return Json(new { success = false, message = $"An error occurred: {ex.Message}" });
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteUser([FromBody] UserDeleteModel model)
        {
            try
            {
                // Find the user
                var user = await _context.Users.FindAsync(model.UserId);
                if (user == null)
                {
                    return Json(new { success = false, message = "User not found." });
                }

                // Check if the user is the last admin
                if (user.Role == "Admin")
                {
                    int adminCount = await _context.Users.CountAsync(u => u.Role == "Admin");
                    if (adminCount <= 1)
                    {
                        return Json(new { success = false, message = "Cannot delete the last admin user." });
                    }
                }

                // Delete the user
                _context.Users.Remove(user);
                await _context.SaveChangesAsync();

                _logger.LogInformation("User deleted: {Username}", user.Username);

                return Json(new { success = true, message = "User deleted successfully." });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting user");
                return Json(new { success = false, message = $"An error occurred: {ex.Message}" });
            }
        }
            [HttpPost]
            [ValidateAntiForgeryToken]
            public async Task<IActionResult> UpdateUserDirect(UserUpdateModel model)
            {
                try
                {
                    _logger.LogInformation("Direct update received for user ID: {UserId}", model.UserId);

                    if (!ModelState.IsValid)
                    {
                        foreach (var error in ModelState.Values.SelectMany(v => v.Errors))
                        {
                            _logger.LogError("Model error: {ErrorMessage}", error.ErrorMessage);
                        }
                        TempData["ErrorMessage"] = "Invalid user data.";
                        return RedirectToAction("UserManagement");
                    }

                    // Find the user
                    var user = await _context.Users.FindAsync(model.UserId);
                    if (user == null)
                    {
                        TempData["ErrorMessage"] = "User not found.";
                        return RedirectToAction("UserManagement");
                    }

                    // Check if username/email is already taken by another user
                    var existingUser = await _context.Users
                        .FirstOrDefaultAsync(u =>
                            u.UserId != model.UserId &&
                            (u.Username == model.Username || u.Email == model.Email));

                    if (existingUser != null)
                    {
                        TempData["ErrorMessage"] = existingUser.Username == model.Username
                            ? "Username is already taken."
                            : "Email is already in use.";
                        return RedirectToAction("UserManagement");
                    }

                    // Update user details
                    user.Username = model.Username;
                    user.Email = model.Email;
                    user.Role = model.Role;
                    user.IsActive = model.Status == "Active";

                    // Update password if requested
                    if (model.ResetPassword && !string.IsNullOrEmpty(model.NewPassword))
                    {
                        user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(model.NewPassword);
                    }

                    _context.Users.Update(user);
                    await _context.SaveChangesAsync();

                    _logger.LogInformation("User updated directly: {Username}", user.Username);

                    TempData["SuccessMessage"] = "User updated successfully.";
                    return RedirectToAction("UserManagement");
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error updating user directly");
                    TempData["ErrorMessage"] = $"An error occurred: {ex.Message}";
                    return RedirectToAction("UserManagement");
                }
            }
        }
    }

    // View Models for User Management
    public class UserCreateModel
    {
        public string Username { get; set; }
        public string Email { get; set; }
        public string Password { get; set; }
        public string Role { get; set; }
    }

    public class UserUpdateModel
    {
        public int UserId { get; set; }
        public string Username { get; set; }
        public string Email { get; set; }
        public string Role { get; set; }
        public string Status { get; set; }
        public bool ResetPassword { get; set; }
        public string NewPassword { get; set; }
    }

    public class UserDeleteModel
    {
        public int UserId { get; set; }
    }
