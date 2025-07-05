using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Opti.Data;
using Opti.Models;
using Stripe;
using Stripe.Checkout;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Opti.Controllers
{
    [Authorize]
    public class PaymentController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IConfiguration _configuration;

        public PaymentController(ApplicationDbContext context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
            StripeConfiguration.ApiKey = _configuration["Stripe:SecretKey"] ?? "sk_test_51RLr7APYeZ7XeGcwEiJpwXP4AA38jmuY4IH2OLnazDAGkHHn5FDosK1T4dhRcvwh8DBJ72QCySdPLtk4egz7yi5Q00a0F7h2A6";
        }

        [HttpPost]
        public async Task<IActionResult> CreateCheckoutSession(string productIds)
        {
            try
            {
                // Debug output
                Console.WriteLine($"CreateCheckoutSession called with productIds: {productIds}");

                if (string.IsNullOrEmpty(productIds))
                {
                    TempData["Error"] = "No products selected for checkout.";
                    return RedirectToAction("Cart", "CustomerDashboard");
                }

                // Parse the comma-separated product IDs
                var productIdList = productIds.Split(',')
                    .Select(id => int.TryParse(id, out int parsedId) ? parsedId : 0)
                    .Where(id => id > 0)
                    .ToList();

                Console.WriteLine($"Parsed product IDs: {string.Join(", ", productIdList)}");

                if (!productIdList.Any())
                {
                    TempData["Error"] = "Invalid product selection.";
                    return RedirectToAction("Cart", "CustomerDashboard");
                }

                // Get the current user ID
                var userId = int.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value);
                Console.WriteLine($"User ID: {userId}");

                // Get cart items from session
                var cartItems = GetCartFromSession();
                Console.WriteLine($"Cart items count: {cartItems?.Count ?? 0}");

                if (cartItems == null || !cartItems.Any())
                {
                    TempData["Error"] = "Your cart is empty.";
                    return RedirectToAction("Cart", "CustomerDashboard");
                }

                // Filter cart items to only include those in the request
                var selectedItems = cartItems.Where(item => productIdList.Contains(item.ProductId)).ToList();
                Console.WriteLine($"Selected items count: {selectedItems?.Count ?? 0}");

                if (!selectedItems.Any())
                {
                    TempData["Error"] = "No valid products found in your selection.";
                    return RedirectToAction("Cart", "CustomerDashboard");
                }

                // Create orders in the database (optional - can be done after payment success)
                var orderIds = new List<int>();
                foreach (var item in selectedItems)
                {
                    var order = new CustomerOrder
                    {
                        ProductId = item.ProductId,
                        Quantity = item.Quantity,
                        TotalAmount = item.Price * item.Quantity,
                        OrderDate = DateTime.Now,
                        UserId = userId
                    };

                    _context.CustomerOrders.Add(order);
                    await _context.SaveChangesAsync(); // Save to get the OrderId

                    orderIds.Add(order.OrderId);
                }
                Console.WriteLine($"Created order IDs: {string.Join(", ", orderIds)}");

                // Create line items for Stripe
                var lineItems = new List<SessionLineItemOptions>();

                foreach (var item in selectedItems)
                {
                    // Calculate amount in smallest currency unit (cents)
                    var amountInSmallestUnit = (long)(item.Price * item.Quantity * 100);

                    lineItems.Add(new SessionLineItemOptions
                    {
                        PriceData = new SessionLineItemPriceDataOptions
                        {
                            UnitAmount = amountInSmallestUnit,
                            Currency = "php",
                            ProductData = new SessionLineItemPriceDataProductDataOptions
                            {
                                Name = item.ProductName,
                                Description = $"Quantity: {item.Quantity}"
                            }
                        },
                        Quantity = 1 // Since each line item already has quantity factored into price
                    });
                }

                // Create success URL with order IDs
                var orderIdsParam = string.Join(",", orderIds);
                var successUrl = Url.Action("PaymentSuccess", "CustomerDashboard",
                    new { orderIds = orderIdsParam },
                    Request.Scheme) + "?session_id={CHECKOUT_SESSION_ID}";

                var cancelUrl = Url.Action("Cart", "CustomerDashboard", null, Request.Scheme);

                Console.WriteLine($"Success URL: {successUrl}");
                Console.WriteLine($"Cancel URL: {cancelUrl}");

                // Create Stripe checkout session
                var options = new SessionCreateOptions
                {
                    PaymentMethodTypes = new List<string> { "card" },
                    LineItems = lineItems,
                    Mode = "payment",
                    SuccessUrl = successUrl,
                    CancelUrl = cancelUrl,
                    CustomerEmail = User.FindFirst(System.Security.Claims.ClaimTypes.Email)?.Value
                };

                var service = new SessionService();
                var session = await service.CreateAsync(options);

                Console.WriteLine($"Created Stripe session ID: {session.Id}");
                Console.WriteLine($"Stripe session URL: {session.Url}");

                // Redirect to Stripe Checkout
                return Redirect(session.Url);
            }
            catch (Exception ex)
            {
                // Log the error
                Console.Error.WriteLine($"Stripe error: {ex.Message}");
                Console.Error.WriteLine($"Stack trace: {ex.StackTrace}");

                if (ex.InnerException != null)
                {
                    Console.Error.WriteLine($"Inner exception: {ex.InnerException.Message}");
                }

                TempData["Error"] = "Payment processing error: " + ex.Message;
                return RedirectToAction("Cart", "CustomerDashboard");
            }
        }

        // Helper method to get cart items from session
        private List<CartItem> GetCartFromSession()
        {
            var session = HttpContext.Session;
            string cartJson = session.GetString("Cart");

            if (string.IsNullOrEmpty(cartJson))
            {
                return new List<CartItem>();
            }

            try
            {
                return System.Text.Json.JsonSerializer.Deserialize<List<CartItem>>(cartJson);
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"Error deserializing cart: {ex.Message}");
                return new List<CartItem>();
            }
        }

        // Cart item model matching CustomerDashboardController.CartItem
        public class CartItem
        {
            public int ProductId { get; set; }
            public string ProductName { get; set; }
            public string Description { get; set; }
            public decimal Price { get; set; }
            public string ImagePath { get; set; }
            public int Quantity { get; set; }
        }
    }
}