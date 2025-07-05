using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Opti.Data;
using Opti.Models;
using Opti.ViewModel;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Opti.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminUserController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<AdminUserController>
    _logger;

    public AdminUserController(ApplicationDbContext context, ILogger<AdminUserController>
        logger)
        {
        _context = context;
        _logger = logger;
        }

        // GET: AdminUser - List all users
        public async Task<IActionResult>
            Index()
            {
            try
            {
            var users = await _context.Users.ToListAsync();
            return View(users);
            }
            catch (Exception ex)
            {
            _logger.LogError(ex, "Error loading users");
            TempData["ErrorMessage"] = "An error occurred while loading users.";
            return RedirectToAction("Index", "AdminDashboard");
            }
            }

            // GET: AdminUser/Details/5
            public async Task<IActionResult>
                Details(int? id)
                {
                if (id == null)
                {
                return NotFound();
                }

                try
                {
                var user = await _context.Users
                .FirstOrDefaultAsync(m => m.UserId == id);

                if (user == null)
                {
                return NotFound();
                }

                return View(user);
                }
                catch (Exception ex)
                {
                _logger.LogError(ex, "Error loading user details");
                TempData["ErrorMessage"] = "An error occurred while loading user details.";
                return RedirectToAction(nameof(Index));
                }
                }

                // GET: AdminUser/Create
                public IActionResult Create()
                {
                return View();
                }

                // POST: AdminUser/Create
                [HttpPost]
                [ValidateAntiForgeryToken]
                public async Task<IActionResult>
                    Create(UserCreateViewModel model)
                    {
                    if (ModelState.IsValid)
                    {
                    try
                    {
                    // Check if username or email already exists
                    var existingUser = await _context.Users
                    .FirstOrDefaultAsync(u => u.Username == model.Username || u.Email == model.Email);

                    if (existingUser != null)
                    {
                    ModelState.AddModelError(string.Empty, "Username or Email is already taken.");
                    return View(model);
                    }

                    var user = new User
                    {
                    Username = SanitizeInput(model.Username),
                    Email = SanitizeInput(model.Email),
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword(model.Password),
                    Role = SanitizeInput(model.Role),
                    CreatedAt = DateTime.UtcNow
                    };

                    _context.Add(user);
                    await _context.SaveChangesAsync();

                    TempData["SuccessMessage"] = "User created successfully.";
                    return RedirectToAction(nameof(Index));
                    }
                    catch (Exception ex)
                    {
                    _logger.LogError(ex, "Error creating user");
                    ModelState.AddModelError(string.Empty, "An error occurred while creating the user.");
                    }
                    }

                    return View(model);
                    }

                    // GET: AdminUser/Edit/5
                    public async Task<IActionResult>
                        Edit(int? id)
                        {
                        if (id == null)
                        {
                        return NotFound();
                        }

                        try
                        {
                        var user = await _context.Users.FindAsync(id);
                        if (user == null)
                        {
                        return NotFound();
                        }

                        var viewModel = new UserEditViewModel
                        {
                        UserId = user.UserId,
                        Username = user.Username,
                        Email = user.Email,
                        Role = user.Role
                        };

                        return View(viewModel);
                        }
                        catch (Exception ex)
                        {
                        _logger.LogError(ex, "Error loading user for edit");
                        TempData["ErrorMessage"] = "An error occurred while loading the user.";
                        return RedirectToAction(nameof(Index));
                        }
                        }

                        // POST: AdminUser/Edit/5
                        [HttpPost]
                        [ValidateAntiForgeryToken]
                        public async Task<IActionResult>
                            Edit(int id, UserEditViewModel model)
                            {
                            if (id != model.UserId)
                            {
                            return NotFound();
                            }

                            if (ModelState.IsValid)
                            {
                            try
                            {
                            // Check if username or email already exists for another user
                            var existingUser = await _context.Users
                            .FirstOrDefaultAsync(u => (u.Username == model.Username || u.Email == model.Email) && u.UserId != id);

                            if (existingUser != null)
                            {
                            ModelState.AddModelError(string.Empty, "Username or Email is already taken by another user.");
                            return View(model);
                            }

                            var user = await _context.Users.FindAsync(id);
                            if (user == null)
                            {
                            return NotFound();
                            }

                            user.Username = SanitizeInput(model.Username);
                            user.Email = SanitizeInput(model.Email);
                            user.Role = SanitizeInput(model.Role);

                            // Only update password if a new one is provided
                            if (!string.IsNullOrEmpty(model.NewPassword))
                            {
                            user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(model.NewPassword);
                            }

                            _context.Update(user);
                            await _context.SaveChangesAsync();

                            TempData["SuccessMessage"] = "User updated successfully.";
                            return RedirectToAction(nameof(Index));
                            }
                            catch (DbUpdateConcurrencyException ex)
                            {
                            if (!UserExists(model.UserId))
                            {
                            return NotFound();
                            }
                            else
                            {
                            _logger.LogError(ex, "Concurrency error updating user");
                            ModelState.AddModelError(string.Empty, "An error occurred while updating the user. The record may have been modified by another user.");
                            }
                            }
                            catch (Exception ex)
                            {
                            _logger.LogError(ex, "Error updating user");
                            ModelState.AddModelError(string.Empty, "An error occurred while updating the user.");
                            }
                            }

                            return View(model);
                            }

                            // GET: AdminUser/Delete/5
                            public async Task<IActionResult>
                                Delete(int? id)
                                {
                                if (id == null)
                                {
                                return NotFound();
                                }

                                try
                                {
                                var user = await _context.Users
                                .FirstOrDefaultAsync(m => m.UserId == id);

                                if (user == null)
                                {
                                return NotFound();
                                }

                                return View(user);
                                }
                                catch (Exception ex)
                                {
                                _logger.LogError(ex, "Error loading user for delete");
                                TempData["ErrorMessage"] = "An error occurred while loading the user.";
                                return RedirectToAction(nameof(Index));
                                }
                                }

                                // POST: AdminUser/Delete/5
                                [HttpPost, ActionName("Delete")]
                                [ValidateAntiForgeryToken]
                                public async Task<IActionResult>
                                    DeleteConfirmed(int id)
                                    {
                                    try
                                    {
                                    var user = await _context.Users.FindAsync(id);
                                    if (user == null)
                                    {
                                    return NotFound();
                                    }

                                    // Check if this is the last admin user to prevent removing all admins
                                    if (user.Role == "Admin")
                                    {
                                    var adminCount = await _context.Users.CountAsync(u => u.Role == "Admin");
                                    if (adminCount <= 1)
                                    {
                                    TempData["ErrorMessage"] = "Cannot delete the last admin user.";
                                    return RedirectToAction(nameof(Index));
                                    }
                                    }

                                    _context.Users.Remove(user);
                                    await _context.SaveChangesAsync();

                                    TempData["SuccessMessage"] = "User deleted successfully.";
                                    return RedirectToAction(nameof(Index));
                                    }
                                    catch (Exception ex)
                                    {
                                    _logger.LogError(ex, "Error deleting user");
                                    TempData["ErrorMessage"] = "An error occurred while deleting the user.";
                                    return RedirectToAction(nameof(Index));
                                    }
                                    }

                                    // AJAX methods for additional functionality
                                    [HttpPost]
                                    [ValidateAntiForgeryToken]
                                    public async Task<IActionResult>
                                        ResetPassword(int id, string newPassword)
                                        {
                                        if (string.IsNullOrEmpty(newPassword))
                                        {
                                        return Json(new { success = false, message = "Password cannot be empty" });
                                        }

                                        try
                                        {
                                        var user = await _context.Users.FindAsync(id);
                                        if (user == null)
                                        {
                                        return Json(new { success = false, message = "User not found" });
                                        }

                                        user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(newPassword);
                                        await _context.SaveChangesAsync();

                                        return Json(new { success = true, message = "Password reset successfully" });
                                        }
                                        catch (Exception ex)
                                        {
                                        _logger.LogError(ex, "Error resetting password");
                                        return Json(new { success = false, message = "An error occurred while resetting the password" });
                                        }
                                        }

                                        // GET Method to check if a username or email already exists
                                        [HttpGet]
                                        public async Task<IActionResult>
                                            CheckUsernameExists(string username, int? userId = null)
                                            {
                                            var query = _context.Users.Where(u => u.Username == username);

                                            if (userId.HasValue)
                                            {
                                            query = query.Where(u => u.UserId != userId.Value);
                                            }

                                            var exists = await query.AnyAsync();
                                            return Json(!exists); // Return true if available (not exists)
                                            }

                                            [HttpGet]
                                            public async Task<IActionResult>
                                                CheckEmailExists(string email, int? userId = null)
                                                {
                                                var query = _context.Users.Where(u => u.Email == email);

                                                if (userId.HasValue)
                                                {
                                                query = query.Where(u => u.UserId != userId.Value);
                                                }

                                                var exists = await query.AnyAsync();
                                                return Json(!exists); // Return true if available (not exists)
                                                }

                                                private bool UserExists(int id)
                                                {
                                                return _context.Users.Any(e => e.UserId == id);
                                                }

                                                // Security: Input sanitization
                                                private string SanitizeInput(string input)
                                                {
                                                if (string.IsNullOrEmpty(input))
                                                return string.Empty;

                                                // Remove potential XSS characters
                                                input = input.Replace("<", "&lt;").Replace(">", "&gt;");

                                                // Trim whitespace
                                                input = input.Trim();

                                                // Limit length
                                                if (input.Length > 255)
                                                input = input.Substring(0, 255);

                                                return input;
                                                }
                                                }
                                                }
