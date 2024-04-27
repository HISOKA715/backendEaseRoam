using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using webCore.Models;

namespace webCore.Controllers
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
            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }
        public IActionResult Notification()
        {
            return View();
        }

        public IActionResult Attraction_management() {
            return View();
        }
        public IActionResult EditUser()
        {
            return View();
        }

        public IActionResult EditAttraction()
        {

            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
        public IActionResult GotoForgetPassword()
        {
            return View("Forgetpassword");
        }
        
    }
}