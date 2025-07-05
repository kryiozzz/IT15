using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Opti.Data;
using Opti.Models;

namespace Opti.Controllers
{
    public class CustomerDashboardController : Controller
    {
        private readonly ApplicationDbContext _context;

        public CustomerDashboardController(ApplicationDbContext context)
        {
            _context = context;
        }

        // Helper method for generating page indication (breadcrumbs)
        private void SetPageIndication(string currentPage, string parentPage = null)
        {
            var breadcrumbs = new List<Breadcrumb>
            {
                new Breadcrumb { Title = "Home", Url = Url.Action("Index", "Home") }
            };

            if (!string.IsNullOrEmpty(parentPage))
            {
                switch (parentPage)
                {
                    case "Products":
                        breadcrumbs.Add(new Breadcrumb { Title = "Products", Url = Url.Action("Index", "CustomerDashboard") });
                        break;
                    case "Machine Status":
                        breadcrumbs.Add(new Breadcrumb { Title = "Machine Status", Url = Url.Action("Machine", "CustomerDashboard") });
                        break;
                    default:
                        breadcrumbs.Add(new Breadcrumb { Title = parentPage, Url = "#" });
                        break;
                }
            }

            breadcrumbs.Add(new Breadcrumb { Title = currentPage, Url = null }); // Current page (no link)

            ViewData["Breadcrumbs"] = breadcrumbs;
        }

        // GET: CustomerProduct
        public async Task<IActionResult> Index(string searchString, int? pageNumber)
        {
            // Set page indication (breadcrumbs)
            SetPageIndication("Products");

            // Set up ViewData for the products UI
            ViewData["Title"] = "Products";
            ViewData["CurrentPage"] = "Products";
            ViewData["SearchQuery"] = searchString;

            // Base query for products
            var products = from p in _context.Products
                           select p;

            // Apply search filter if provided
            if (!string.IsNullOrEmpty(searchString))
            {
                // Enhanced search - split the search string to search for multiple terms
                var searchTerms = searchString.ToLower().Split(' ');

                products = products.Where(p =>
                    searchTerms.All(term =>
                        p.ProductName.ToLower().Contains(term) ||
                        p.Description.ToLower().Contains(term)
                    )
                );
            }

            // For now, return all products, but you might want to add pagination later
            var productList = await products.OrderByDescending(p => p.CreatedAt).ToListAsync();

            return View(productList);
        }

        // GET: CustomerProduct/Search (handle the search form submission)
        public async Task<IActionResult> Search(string searchString)
        {
            // Redirect to Index with search parameter
            return RedirectToAction("Index", new { searchString = searchString });
        }

        // GET: CustomerProduct/Details/5
        // GET: CustomerDashboard/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var product = await _context.Products
                .FirstOrDefaultAsync(m => m.ProductId == id);
            if (product == null)
            {
                return NotFound();
            }

            // Return JSON data instead of a View
            return Json(new
            {
                productId = product.ProductId,
                productName = product.ProductName,
                description = product.Description,
                price = product.Price,
                stockQuantity = product.StockQuantity,
                createdAt = product.CreatedAt,
                imagePath = product.ImagePath
            });
        }

        // GET: CustomerProduct/Create
        public IActionResult Create()
        {
            // Set page indication (breadcrumbs) with parent
            SetPageIndication("Create New Product", "Products");
            ViewData["CurrentPage"] = "Create Product";

            return View();
        }

        // POST: CustomerProduct/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("ProductId,ProductName,Description,Price,StockQuantity,CreatedAt,ImagePath,Category,Tags")] Product product)
        {
            // Set page indication (breadcrumbs) with parent
            SetPageIndication("Create New Product", "Products");
            ViewData["CurrentPage"] = "Create Product";

            if (ModelState.IsValid)
            {
                _context.Add(product);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(product);
        }
        [Authorize]
        public async Task<IActionResult> Check(IConfiguration _configuration)
        {
            // Get the current user ID
            var userId = int.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value);

            // Get unpaid orders for this user
            var orders = await _context.CustomerOrders
                .Where(o => o.UserId == userId)
                .ToListAsync();

            if (!orders.Any())
            {
                return RedirectToAction("CustomerDashboard");
            }

            // Make the Stripe publishable key available to the view
            ViewBag.StripePublishableKey = _configuration["Stripe:pk_test_51RLr7APYeZ7XeGcweHozfkKMXuwW2xPJ12zBQzzGNZIScsTIE0cu6zpoMARDTliCWbZZzj0FS0RWGR4C2OcRGTUZ006zCroFxl"];

            return View(orders);
        }



        // GET: CustomerProduct/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var product = await _context.Products.FindAsync(id);
            if (product == null)
            {
                return NotFound();
            }

            // Set page indication (breadcrumbs) with parent
            SetPageIndication($"Edit {product.ProductName}", "Products");
            ViewData["CurrentPage"] = "Edit Product";

            return View(product);
        }

        // POST: CustomerProduct/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("ProductId,ProductName,Description,Price,StockQuantity,CreatedAt,ImagePath,Category,Tags")] Product product)
        {
            // Set page indication (breadcrumbs) with parent
            SetPageIndication($"Edit {product.ProductName}", "Products");
            ViewData["CurrentPage"] = "Edit Product";

            if (id != product.ProductId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(product);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ProductExists(product.ProductId))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            return View(product);
        }

        // GET: CustomerProduct/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var product = await _context.Products
                .FirstOrDefaultAsync(m => m.ProductId == id);
            if (product == null)
            {
                return NotFound();
            }

            // Set page indication (breadcrumbs) with parent
            SetPageIndication($"Delete {product.ProductName}", "Products");
            ViewData["CurrentPage"] = "Delete Product";

            return View(product);
        }

        // POST: CustomerProduct/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var product = await _context.Products.FindAsync(id);
            if (product != null)
            {
                _context.Products.Remove(product);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool ProductExists(int id)
        {
            return _context.Products.Any(e => e.ProductId == id);
        }

        // GET: CustomerDashboard/Machine
        public async Task<IActionResult> Machine(string searchString)
        {
            // Set page indication (breadcrumbs)
            SetPageIndication("Machine Status");

            // Set up ViewData for the machine UI
            ViewData["Title"] = "Machine Status";
            ViewData["CurrentPage"] = "Machine Status";
            ViewData["SearchQuery"] = searchString;

            // Get all machines by calling the MachinesController
            var machinesController = new MachinesController(_context);
            var machinesResult = await machinesController.Index() as ViewResult;
            var machines = machinesResult?.Model as List<Machine>;

            // Apply search filter if provided
            if (!string.IsNullOrEmpty(searchString) && machines != null)
            {
                // Enhanced search - split the search string to search for multiple terms
                var searchTerms = searchString.ToLower().Split(' ');

                machines = machines.Where(m =>
                    searchTerms.All(term =>
                        m.MachineName.ToLower().Contains(term) ||
                        m.MachineType.ToLower().Contains(term) ||
                        m.Status.ToLower().Contains(term)
                    )
                ).ToList();
            }

            return View(machines);
        }

        // GET: CustomerDashboard/GetMachineDetails/5
        [HttpGet]
        public async Task<IActionResult> GetMachineDetails(int machineId)
        {
            // Call MachinesController's GetMachineDetails method
            var machinesController = new MachinesController(_context);
            var result = await machinesController.GetMachineDetails(machineId);

            // Just return the result as is since it's a JsonResult
            return result;
        }

        // POST: CustomerDashboard/AddToCart
        [HttpPost]
        public async Task<IActionResult> AddToCart(int productId)
        {
            // Check if user is logged in
            if (!IsUserLoggedIn())
            {
                return Json(new { success = false, message = "Please log in to add items to your cart", requireLogin = true });
            }

            // Find the product
            var product = await _context.Products.FindAsync(productId);
            if (product == null)
            {
                return Json(new { success = false, message = "Product not found" });
            }

            // Get current cart
            var cartItems = GetCartFromSession();

            // Check if product already in cart
            var existingItem = cartItems.FirstOrDefault(item => item.ProductId == productId);
            if (existingItem != null)
            {
                // Product already in cart, increase quantity
                existingItem.Quantity += 1;
            }
            else
            {
                // Add new product to cart
                cartItems.Add(new CartItem
                {
                    ProductId = product.ProductId,
                    ProductName = product.ProductName,
                    Description = product.Description,
                    Price = product.Price,
                    ImagePath = product.ImagePath,
                    Quantity = 1
                });
            }

            // Save updated cart back to session
            SaveCartToSession(cartItems);
            return Json(new { success = true, message = "Product added to cart", cartCount = cartItems.Count });
        }

        // POST: CustomerDashboard/UpdateCartQuantity
        [HttpPost]
        public IActionResult UpdateCartQuantity(int productId, int change)
        {
            // Check if user is logged in
            if (!IsUserLoggedIn())
            {
                return Json(new { success = false, message = "Please log in to update your cart", requireLogin = true });
            }

            var cartItems = GetCartFromSession();
            var item = cartItems.FirstOrDefault(i => i.ProductId == productId);
            if (item == null)
            {
                return Json(new { success = false, message = "Product not found in cart" });
            }

            item.Quantity += change;

            // Remove item if quantity reaches 0
            if (item.Quantity <= 0)
            {
                cartItems.Remove(item);
            }

            SaveCartToSession(cartItems);
            var totalPrice = cartItems.Sum(i => i.Price * i.Quantity);

            return Json(new
            {
                success = true,
                newQuantity = item.Quantity > 0 ? item.Quantity : 0,
                itemSubtotal = item.Quantity * item.Price,
                cartTotal = totalPrice,
                itemCount = cartItems.Count
            });
        }

        // POST: CustomerDashboard/RemoveFromCart
        [HttpPost]
        public IActionResult RemoveFromCart(int productId)
        {
            // Check if user is logged in
            if (!IsUserLoggedIn())
            {
                return Json(new { success = false, message = "Please log in to update your cart", requireLogin = true });
            }

            var cartItems = GetCartFromSession();
            var item = cartItems.FirstOrDefault(i => i.ProductId == productId);
            if (item != null)
            {
                cartItems.Remove(item);
                SaveCartToSession(cartItems);
            }

            var totalPrice = cartItems.Sum(i => i.Price * i.Quantity);

            return Json(new
            {
                success = true,
                message = "Item removed from cart",
                cartTotal = totalPrice,
                itemCount = cartItems.Count
            });
        }
        [Authorize]
        public async Task<IActionResult> PaymentSuccess(string orderIds, string session_id)
        {
            // Verify the user is logged in
            if (!User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Login", "Account");
            }

            var userId = int.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value);

            // Check if we have order IDs
            if (string.IsNullOrEmpty(orderIds))
            {
                TempData["Error"] = "No order information was found.";
                return RedirectToAction("Index");
            }

            var orderIdList = orderIds.Split(',')
                .Select(id => int.TryParse(id, out int parsedId) ? parsedId : 0)
                .Where(id => id > 0)
                .ToList();

            // Get the orders
            var orders = await _context.CustomerOrders
                .Include(o => o.Product)
                .Where(o => orderIdList.Contains(o.OrderId) && o.UserId == userId)
                .ToListAsync();

            if (!orders.Any())
            {
                TempData["Error"] = "No valid orders were found.";
                return RedirectToAction("Index");
            }

            // Create payment records
            foreach (var order in orders)
            {
                // Create a payment record
                var payment = new Payment
                {
                    OrderId = order.OrderId,
                    UserId = userId,
                    Amount = order.TotalAmount,
                    PaymentDate = DateTime.Now,
                    PaymentStatus = "Completed",
                    PaymentMethod = "Credit Card",
                    TransactionExternalId = session_id,
                    Gateway = "Stripe",
                    Status = "Success"
                };

                _context.Payments.Add(payment);
            }

            // Save changes to database
            await _context.SaveChangesAsync();

            // Update cart - clear the cart after successful payment
            var session = HttpContext.Session;
            session.Remove("Cart");

            // Set ViewBag data for the view
            ViewBag.SessionId = session_id;
            ViewBag.TotalAmount = orders.Sum(o => o.TotalAmount);
            ViewBag.OrderCount = orders.Count;

            TempData["Success"] = "Payment completed successfully!";
            return View(orders);
        }




        // GET: CustomerDashboard/Cart
        public IActionResult Cart()
        {
            // Set page indication (breadcrumbs)
            SetPageIndication("Shopping Cart");

            ViewData["Title"] = "Your Cart";
            ViewData["CurrentPage"] = "Cart";

            // User authentication check is now done in the view
            // The view will show appropriate content based on login status

            // Only try to get cart data if user is logged in
            if (User.Identity.IsAuthenticated)
            {
                // Get cart items from session
                var cartItems = GetCartFromSession();

                // Calculate total price
                ViewBag.TotalPrice = cartItems.Sum(item => item.Price * item.Quantity);

                return View(cartItems);
            }

            // Return empty model for not logged in users
            // The view will show the login message instead of cart contents
            return View(new List<CartItem>());
        }

        // Helper method to check if user is logged in
        private bool IsUserLoggedIn()
        {
            // You may need to adjust this based on your authentication system
            return User.Identity.IsAuthenticated;
        }

        // Helper methods for session management
        private List<CartItem> GetCartFromSession()
        {
            var session = HttpContext.Session;
            string cartJson = session.GetString("Cart");

            if (string.IsNullOrEmpty(cartJson))
            {
                return new List<CartItem>();
            }

            return System.Text.Json.JsonSerializer.Deserialize<List<CartItem>>(cartJson);
        }

        private void SaveCartToSession(List<CartItem> cart)
        {
            var session = HttpContext.Session;
            string cartJson = System.Text.Json.JsonSerializer.Serialize(cart);
            session.SetString("Cart", cartJson);
        }

        // GET: CustomerDashboard/GetCartCount
        [HttpGet]
        public IActionResult GetCartCount()
        {
            // Check if user is logged in
            if (!IsUserLoggedIn())
            {
                return Json(new { count = 0 });
            }

            var cartItems = GetCartFromSession();
            return Json(new { count = cartItems.Count });
        }

        // Temporary class to represent cart items (internal to the controller)
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

    // Breadcrumb class for page indication
    public class Breadcrumb
    {
        public string Title { get; set; }
        public string Url { get; set; }
    }
}