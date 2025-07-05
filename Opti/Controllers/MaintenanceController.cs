using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Opti.Data;
using Opti.Models;
using Opti.ViewModels;

namespace Opti.Controllers
{
    public class MaintenanceController : Controller
    {
        private readonly ApplicationDbContext _context;

        public MaintenanceController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Maintenance
        public async Task<IActionResult> Index()
        {
            // Get machines that are under maintenance
            var machinesUnderMaintenance = await _context.Machines
                .Where(m => m.Status == "Under Maintenance")
                .OrderByDescending(m => m.LastMaintenanceDate)
                .ToListAsync();

            // Get the maintenance logs for these machines
            var maintenanceLogs = await _context.MachineLogs
                .Where(ml => machinesUnderMaintenance.Select(m => m.MachineId).Contains(ml.MachineId))
                .OrderByDescending(ml => ml.Timestamp)
                .ToListAsync();

            // Group logs by machine ID for easy access
            var logsByMachine = maintenanceLogs
                .GroupBy(ml => ml.MachineId)
                .ToDictionary(g => g.Key, g => g.ToList());

            // Create the view model
            var viewModel = new MaintenanceViewModel
            {
                MachinesUnderMaintenance = machinesUnderMaintenance,
                MaintenanceLogs = logsByMachine,
                MaintenanceCount = machinesUnderMaintenance.Count
            };

            return View("~/Views/WorkerDashboard/WorkerMaintenance.cshtml", viewModel);
        }

        // POST: Maintenance/CompleteMaintenance
        [HttpPost]
        public async Task<IActionResult> CompleteMaintenance(int machineId, string workDone, string notes)
        {
            var machine = await _context.Machines.FindAsync(machineId);

            if (machine == null)
            {
                return NotFound();
            }

            // Update machine status
            machine.Status = "Operational";
            machine.LastMaintenanceDate = DateTime.Now;

            // Create a log entry
            var machineLog = new MachineLog
            {
                MachineId = machineId,
                Action = $"Maintenance completed: {workDone}",
                Timestamp = DateTime.Now,
                UserId = 1 // In a real app, this would be the logged-in user ID
            };

            _context.MachineLogs.Add(machineLog);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        // POST: Maintenance/ExtendMaintenance
        [HttpPost]
        public async Task<IActionResult> ExtendMaintenance(int machineId, string reason, DateTime estimatedCompletion)
        {
            var machine = await _context.Machines.FindAsync(machineId);

            if (machine == null)
            {
                return NotFound();
            }

            // Create a log entry
            var machineLog = new MachineLog
            {
                MachineId = machineId,
                Action = $"Maintenance extended: {reason}. New estimated completion: {estimatedCompletion.ToShortDateString()}",
                Timestamp = DateTime.Now,
                UserId = 1 // In a real app, this would be the logged-in user ID
            };

            _context.MachineLogs.Add(machineLog);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        // POST: Maintenance/AddLog
        [HttpPost]
        public async Task<IActionResult> AddLog(int machineId, string logType, string logDetails)
        {
            var machine = await _context.Machines.FindAsync(machineId);

            if (machine == null)
            {
                return NotFound();
            }

            // Create a log entry
            var machineLog = new MachineLog
            {
                MachineId = machineId,
                Action = $"{logType}: {logDetails}",
                Timestamp = DateTime.Now,
                UserId = 1 // In a real app, this would be the logged-in user ID
            };

            _context.MachineLogs.Add(machineLog);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        // POST: Maintenance/OrderParts
        [HttpPost]
        public async Task<IActionResult> OrderParts(int machineId, string parts, int quantity, string urgency)
        {
            var machine = await _context.Machines.FindAsync(machineId);

            if (machine == null)
            {
                return NotFound();
            }

            // Create a log entry for parts order
            var machineLog = new MachineLog
            {
                MachineId = machineId,
                Action = $"Parts ordered: {parts} (Qty: {quantity}). Urgency: {urgency}",
                Timestamp = DateTime.Now,
                UserId = 1 // In a real app, this would be the logged-in user ID
            };

            _context.MachineLogs.Add(machineLog);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }
    }
}