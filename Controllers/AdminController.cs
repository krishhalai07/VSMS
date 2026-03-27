using Microsoft.AspNetCore.Mvc;

namespace VSMS.Controllers
{
    public class AdminController : Controller
    {
        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Login(string email, string password)
        {
            // For UI purposes, redirect straight to dashboard
            return RedirectToAction("Dashboard");
        }

        public IActionResult Dashboard()
        {
            return View();
        }

        public IActionResult Profile()
        {
            return View();
        }

        public IActionResult Customers()
        {
            return View();
        }

        public IActionResult Vehicles()
        {
            return View();
        }

        public IActionResult Mechanics()
        {
            return View();
        }

        public IActionResult JobCards()
        {
            return View();
        }


        public IActionResult Billing()
        {
            return View();
        }

        public IActionResult Reports()
        {
            return View();
        }
    }
}
