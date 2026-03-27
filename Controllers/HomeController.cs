using Microsoft.AspNetCore.Mvc;

namespace VSMS.Controllers
{
    public class HomeController : Controller
    {
        public IActionResult Index() => RedirectToAction("Login", "Auth");
    }
}
