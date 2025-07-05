using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Opti.Data;
using Opti.Models;

namespace Opti.Controllers
{
    public class MachinesController : Controller
    {
        private readonly ApplicationDbContext _context;

        public MachinesController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Machines
        public async Task<IActionResult> Index()
        {
            var machines = await _context.Machines
                .OrderBy(m => m.MachineName)
                .ToListAsync();

            return View("~/Views/WorkerDashboard/WorkerMachines.cshtml", machines);
        }

        [HttpPost]
        public async Task<IActionResult> UpdateStatus()
        {
            try
            {
                // Get values directly from form data for more reliability
                if (!int.TryParse(Request.Form["machineId"], out int machineId))
                {
                    return Json(new { success = false, message = "Invalid machine ID" });
                }

                string status = Request.Form["status"];

                Console.WriteLine($"UpdateStatus called with machineId: {machineId}, status: {status}");

                // Check if status is valid
                if (string.IsNullOrEmpty(status))
                {
                    Console.WriteLine("Status is null or empty");
                    return Json(new { success = false, message = "Please select a valid status" });
                }

                // Rest of your existing code...

                // Normalize the status to match the allowed values in the CHECK constraint
                string normalizedStatus;
                switch (status.ToLower().Replace(" ", ""))
                {
                    case "operational":
                        normalizedStatus = "Operational";
                        break;
                    case "undermaintenance":
                    case "under maintenance":
                    case "maintenance":
                        normalizedStatus = "Under Maintenance";
                        break;
                    case "offline":
                        normalizedStatus = "Offline";
                        break;
                    default:
                        Console.WriteLine($"Invalid status value: '{status}'");
                        return Json(new { success = false, message = $"Please select a valid status. Received: '{status}'" });
                }

                // Get the machine
                var machine = await _context.Machines.FindAsync(machineId);
                if (machine == null)
                {
                    Console.WriteLine($"Machine with ID {machineId} not found");
                    return Json(new { success = false, message = "Machine not found." });
                }

                // Update machine status and last maintenance date
                machine.Status = normalizedStatus;
                machine.LastMaintenanceDate = DateTime.Now;

                // Create a machine log entry
                try
                {
                    var machineLog = new MachineLog
                    {
                        MachineId = machineId,
                        Timestamp = DateTime.Now,
                        Action = $"Status updated to {normalizedStatus}",
                        UserId = 1 // Replace with actual user ID if available
                    };

                    _context.MachineLogs.Add(machineLog);
                }
                catch (Exception logEx)
                {
                    // If adding the log fails, just log the error but continue with the status update
                    Console.WriteLine($"Error adding machine log: {logEx.ToString()}");
                }

                // Save changes
                await _context.SaveChangesAsync();
                Console.WriteLine("Changes saved successfully");

                return Json(new { success = true, message = $"Machine status updated to {normalizedStatus} successfully" });
            }
            catch (Exception ex)
            {
                // Log the full exception details
                Console.WriteLine($"Error in UpdateStatus: {ex.ToString()}");

                if (ex.InnerException != null)
                {
                    return Json(new { success = false, message = $"Database error: {ex.InnerException.Message}" });
                }

                return Json(new { success = false, message = $"Error: {ex.Message}" });
            }
        }
        // GET: Machines/GetMachineDetails
        [HttpGet]
        public async Task<IActionResult> GetMachineDetails(int machineId)
        {
            try
            {
                var machine = await _context.Machines
                    .FirstOrDefaultAsync(m => m.MachineId == machineId);

                if (machine == null)
                {
                    return Json(new { success = false, message = "Machine not found" });
                }

                // Get maintenance history from MachineLogs
                var maintenanceHistory = await _context.MachineLogs
                    .Where(ml => ml.MachineId == machineId)
                    .Include(ml => ml.User)
                    .OrderByDescending(ml => ml.Timestamp)
                    .Take(10)
                    .Select(ml => new
                    {
                        date = ml.Timestamp.ToShortDateString(),
                        action = ml.Action,
                        performedBy = ml.User != null ? ml.User.Username : "Unknown User"
                    })
                    .ToListAsync();

                // Calculate efficiency (placeholder - you can replace with actual calculation)
                var efficiency = new Random().Next(70, 95);

                return Json(new
                {
                    success = true,
                    machineId = machine.MachineId,
                    machineName = machine.MachineName,
                    machineType = machine.MachineType,
                    status = machine.Status,
                    lastMaintenance = machine.LastMaintenanceDate.ToShortDateString(),
                    efficiency = $"{efficiency}%",
                    imagePath = machine.ImagePath ?? "/images/machines/placeholder.jpg",
                    maintenanceHistory = maintenanceHistory
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in GetMachineDetails: {ex.Message}");
                return Json(new { success = false, message = $"Error: {ex.Message}" });
            }
        }

        // POST: Machines/LogIssue
        [HttpPost]
        public async Task<IActionResult> LogIssue(int machineId, string issueType, string severity, string description)
        {
            try
            {
                var machine = await _context.Machines.FindAsync(machineId);

                if (machine == null)
                {
                    return Json(new { success = false, message = "Machine not found" });
                }

                // Create a machine log entry for the issue
                var machineLog = new MachineLog
                {
                    MachineId = machineId,
                    Timestamp = DateTime.Now,
                    Action = $"Issue Reported - {issueType} (Severity: {severity}): {description}",
                    UserId = 1 // Replace with actual user ID
                };

                _context.MachineLogs.Add(machineLog);

                // If the issue is high or critical, mark the machine as offline
                if (severity == "High" || severity == "Critical")
                {
                    machine.Status = "Offline";

                    // Add another log entry for the status change
                    var statusLog = new MachineLog
                    {
                        MachineId = machineId,
                        Timestamp = DateTime.Now,
                        Action = $"Status Changed to Offline - Due to {severity.ToLower()} severity issue: {issueType}",
                        UserId = 1 // Replace with actual user ID
                    };

                    _context.MachineLogs.Add(statusLog);
                }

                await _context.SaveChangesAsync();

                return Json(new
                {
                    success = true,
                    message = "Issue logged successfully",
                    shouldMarkOffline = severity == "High" || severity == "Critical"
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in LogIssue: {ex.Message}");
                return Json(new { success = false, message = $"Error: {ex.Message}" });
            }
        }

        // Optional: Method to get machine statistics
        [HttpGet]
        public async Task<IActionResult> GetMachineStatistics()
        {
            try
            {
                var machines = await _context.Machines.ToListAsync();

                var stats = new
                {
                    totalMachines = machines.Count,
                    operational = machines.Count(m => m.Status == "Operational"),
                    underMaintenance = machines.Count(m => m.Status == "Under Maintenance"),
                    offline = machines.Count(m => m.Status == "Offline")
                };

                return Json(new { success = true, stats = stats });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in GetMachineStatistics: {ex.Message}");
                return Json(new { success = false, message = $"Error: {ex.Message}" });
            }
        }
    }
}