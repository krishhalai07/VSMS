using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace VSMS.Controllers
{
    public class CustomerController : Controller
    {
        // GET: CustomerController
        public ActionResult Index()
        {
            return View();
        }

        // GET: CustomerController/Details/5
        public ActionResult Details(int id)
        {
            return View();
        }

        // GET: CustomerController/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: CustomerController/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(IFormCollection collection)
        {
            try
            {
                return RedirectToAction(nameof(Index));
            }
            catch
            {
                return View();
            }
        }

        // GET: CustomerController/Edit/5
        public ActionResult Edit(int id)
        {
            return View();
        }

        // POST: CustomerController/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(int id, IFormCollection collection)
        {
            try
            {
                return RedirectToAction(nameof(Index));
            }
            catch
            {
                return View();
            }
        }

        // GET: CustomerController/Delete/5
        public ActionResult Delete(int id)
        {
            return View();
        }

        // POST: CustomerController/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Delete(int id, IFormCollection collection)
        {
            try
            {
                return RedirectToAction(nameof(Index));
            }
            catch
            {
                return View();
            }
        }
        // GET: CustomerController/Login
        public ActionResult Login()
        {
            return View();
        }

        // POST: CustomerController/Login
        [HttpPost]
        public ActionResult Login(string Email, string Pass)
        {
            // Add actual login logic here later
            return View();
        }

        // GET: CustomerController/Register
        public ActionResult Register()
        {
            return View();
        }

        // POST: CustomerController/Register
        [HttpPost]
        public ActionResult Register(IFormCollection collection)
        {
            // Add actual registration logic here later
            return View();
        }

        public ActionResult Home()
        {
            return View();
        }

        public ActionResult About()
        {
            return View();
        }

        public ActionResult RequestService()
        {
            return View();
        }

        public ActionResult MyRequests()
        {
            return View();
        }

        public ActionResult Profile()
        {
            return View();
        }
    }
}
