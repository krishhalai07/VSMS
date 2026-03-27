using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;

namespace VSMS.Controllers
{
    [IgnoreAntiforgeryToken]
    public class AuthController : Controller
    {
        private readonly IConfiguration _config;
        private readonly DB _db;

        public AuthController(IConfiguration config, DB db)
        {
            _config = config;
            _db = db;
        }

        [HttpGet]
        public IActionResult Login() => View();

        [HttpPost]
        public IActionResult Login(string email, string password)
        {
            // 1. Check admin credentials from config
            if (email == _config["AdminCredentials:Email"] &&
                password == _config["AdminCredentials:Password"])
            {
                HttpContext.Session.SetString("Role", "Admin");
                return RedirectToAction("Dashboard", "Admin");
            }

            // 2. Check customer credentials from DB
            try
            {
                using var conn = _db.Open();
                using var cmd = new SqlCommand(
                    "SELECT Customer_id, Name FROM Customer WHERE Email=@e AND Pass=@p", conn);
                cmd.Parameters.AddWithValue("@e", email);
                cmd.Parameters.AddWithValue("@p", password);
                using var r = cmd.ExecuteReader();
                if (r.Read())
                {
                    HttpContext.Session.SetString("Role", "Customer");
                    HttpContext.Session.SetInt32("CustomerId", r.GetInt32(0));
                    HttpContext.Session.SetString("CustomerName", r.IsDBNull(1) ? "Customer" : r.GetString(1));
                    return RedirectToAction("Home", "Customer");
                }
            }
            catch (Exception ex)
            {
                ViewBag.Error = "Database error: " + ex.Message;
                return View();
            }

            ViewBag.Error = "Invalid email or password.";
            return View();
        }

        [HttpPost]
        public IActionResult Register(string Name, string Email, string Contact, string Pass)
        {
            try
            {
                using var conn = _db.Open();
                using (var chk = new SqlCommand("SELECT COUNT(1) FROM Customer WHERE Email=@e", conn))
                {
                    chk.Parameters.AddWithValue("@e", Email);
                    if ((int)chk.ExecuteScalar() > 0)
                    {
                        TempData["RegError"] = "This email is already registered.";
                        TempData["ShowReg"] = "1";
                        return RedirectToAction("Login");
                    }
                }
                using var cmd = new SqlCommand(
                    "INSERT INTO Customer(Name,Email,Contact,Pass) VALUES(@n,@e,@c,@p)", conn);
                cmd.Parameters.AddWithValue("@n", Name ?? "");
                cmd.Parameters.AddWithValue("@e", Email ?? "");
                cmd.Parameters.AddWithValue("@c", Contact ?? "");
                cmd.Parameters.AddWithValue("@p", Pass ?? "");
                cmd.ExecuteNonQuery();

                TempData["RegSuccess"] = "Account created! You can now sign in.";
            }
            catch (Exception ex)
            {
                TempData["RegError"] = "Error: " + ex.Message;
                TempData["ShowReg"] = "1";
            }
            return RedirectToAction("Login");
        }

        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            return RedirectToAction("Login");
        }
    }
}
