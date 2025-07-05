using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Opti.Data;
using Opti.Models;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Opti.Controllers
{
    public class WorkerDashboardController : Controller
    {
        private readonly ApplicationDbContext _context;

        public WorkerDashboardController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            // Create view model for dashboard
            var viewModel = new WorkerDashboardViewModel
            {
                // Get operational machines count
                MachinesOperational = await _context.Machines
                    .CountAsync(m => m.Status == "Operational"),

                // Get machines under maintenance count
                MachinesUnderMaintenance = await _context.Machines
                    .CountAsync(m => m.Status == "Under Maintenance"),

                // Get completed production orders count
                ProductionOrdersCompleted = await _context.ProductionOrders
                    .CountAsync(o => o.Status == "Completed"),

                // Get in-progress production orders count
                ProductionOrdersInProgress = await _context.ProductionOrders
                    .CountAsync(o => o.Status == "In Progress"),

                // Get all machines for table
                Machine = await _context.Machines
                    .OrderByDescending(m => m.CreatedAt)
                    .ToListAsync(),

                // Get recent production orders for table
                ProductionOrders = await _context.ProductionOrders
                    .Include(o => o.Product)
                    .OrderByDescending(o => o.OrderDate)
                    .Take(10)
                    .ToListAsync()
            };

            return View(viewModel);
        }
    }
}