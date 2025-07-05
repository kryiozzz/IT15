using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Opti.Models;

namespace Opti.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;

        public HomeController(ILogger<HomeController> logger)
        {
            _logger = logger;
        }

        public IActionResult Index()
        {
            // If user is authenticated, redirect to appropriate dashboard
            if (User.Identity?.IsAuthenticated == true)
            {
                if (User.IsInRole("Admin"))
                {
                    _logger.LogInformation("Home: Redirecting authenticated admin user to AdminDashboard");
                    return RedirectToAction("Index", "AdminDashboard");
                }
                else if (User.IsInRole("Worker"))
                {
                    _logger.LogInformation("Home: Redirecting authenticated worker user to WorkerDashboard");
                    return RedirectToAction("Index", "WorkerDashboard");
                }
            }

            // Otherwise show the home page for non-authenticated users
            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}