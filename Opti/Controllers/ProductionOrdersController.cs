using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Opti.Data;
using Opti.Models;

namespace Opti.Controllers
{
    public class ProductionOrdersController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ProductionOrdersController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: ProductionOrders
        public async Task<IActionResult> Index()
        {
            var productionOrders = await _context.ProductionOrders
                .Include(p => p.Product)
                .Include(p => p.User)
                .OrderByDescending(p => p.OrderDate)
                .ToListAsync();

            return View("~/Views/WorkerDashboard/WorkerProductionOrders.cshtml", productionOrders);
        }

        // POST: ProductionOrders/StartProduction
        [HttpPost]
        public async Task<IActionResult> StartProduction(int orderId, DateTime? estimatedCompletionDate = null)
        {
            try
            {
                var order = await _context.ProductionOrders.FindAsync(orderId);

                if (order == null)
                {
                    return Json(new { success = false, message = "Order not found" });
                }

                // Update order status to "In Progress"
                order.Status = "In Progress";

                // Save the changes
                await _context.SaveChangesAsync();

                // Return JSON response for AJAX calls
                return Json(new { success = true, message = "Production started successfully" });
            }
            catch (Exception ex)
            {
                // Log the exception
                Console.WriteLine($"Error in StartProduction: {ex.Message}");
                return Json(new { success = false, message = $"Error: {ex.Message}" });
            }
        }

        // POST: ProductionOrders/CompleteProduction
        [HttpPost]
        public async Task<IActionResult> CompleteProduction(int orderId, int actualQuantity)
        {
            try
            {
                var order = await _context.ProductionOrders.FindAsync(orderId);

                if (order == null)
                {
                    return Json(new { success = false, message = "Order not found" });
                }

                // Update order status to "Completed"
                order.Status = "Completed";

                // Save the changes
                await _context.SaveChangesAsync();

                // Return JSON response for AJAX calls
                return Json(new { success = true, message = "Production completed successfully" });
            }
            catch (Exception ex)
            {
                // Log the exception
                Console.WriteLine($"Error in CompleteProduction: {ex.Message}");
                return Json(new { success = false, message = $"Error: {ex.Message}" });
            }
        }

        // GET: ProductionOrders/GetOrderDetails
        [HttpGet]
        public async Task<IActionResult> GetOrderDetails(int orderId)
        {
            try
            {
                var order = await _context.ProductionOrders
                    .Include(p => p.Product)
                    .Include(p => p.User)
                    .FirstOrDefaultAsync(m => m.OrderId == orderId);

                if (order == null)
                {
                    return Json(new { success = false, message = "Order not found" });
                }

                // Return order details as JSON for modal population
                return Json(new
                {
                    success = true,
                    orderId = order.OrderId,
                    productName = order.Product?.ProductName ?? "Unknown Product",
                    quantity = order.Quantity,
                    status = order.Status,
                    orderDate = order.OrderDate.ToShortDateString(),
                    assignedTo = order.User?.Username ?? "Unassigned"
                });
            }
            catch (Exception ex)
            {
                // Log the exception
                Console.WriteLine($"Error in GetOrderDetails: {ex.Message}");
                return Json(new { success = false, message = $"Error: {ex.Message}" });
            }
        }

        // Other existing methods...
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var productionOrder = await _context.ProductionOrders
                .Include(p => p.Product)
                .Include(p => p.User)
                .FirstOrDefaultAsync(m => m.OrderId == id);

            if (productionOrder == null)
            {
                return NotFound();
            }

            return View(productionOrder);
        }
    }
}