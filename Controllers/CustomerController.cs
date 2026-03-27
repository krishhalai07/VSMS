using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using VSMS.Models;

namespace VSMS.Controllers
{
    [IgnoreAntiforgeryToken]
    public class CustomerController : Controller
    {
        private readonly DB _db;
        public CustomerController(DB db) => _db = db;

        private bool IsLoggedIn => HttpContext.Session.GetString("Role") == "Customer";
        private IActionResult Guard() => RedirectToAction("Login", "Auth");

        // ── HOME ────────────────────────────────────────────────
        public IActionResult Home()
        {
            if (!IsLoggedIn) return Guard();
            ViewBag.CustomerName = HttpContext.Session.GetString("CustomerName");
            return View();
        }

        // ── ABOUT ───────────────────────────────────────────────
        public IActionResult About()
        {
            if (!IsLoggedIn) return Guard();
            return View();
        }

        // ── PROFILE ─────────────────────────────────────────────
        public IActionResult Profile()
        {
            if (!IsLoggedIn) return Guard();
            int id = HttpContext.Session.GetInt32("CustomerId") ?? 0;
            Customer? cust = null;
            using var conn = _db.Open();
            using var cmd = new SqlCommand("SELECT Customer_id,Name,Email,Contact FROM Customer WHERE Customer_id=@id", conn);
            cmd.Parameters.AddWithValue("@id", id);
            using var r = cmd.ExecuteReader();
            if (r.Read())
                cust = new Customer {
                    Customer_id = r.GetInt32(0),
                    Name    = r.IsDBNull(1) ? "" : r.GetString(1),
                    Email   = r.IsDBNull(2) ? "" : r.GetString(2),
                    Contact = r.IsDBNull(3) ? "" : r.GetString(3)
                };
            return View(cust);
        }

        // ── REQUEST SERVICE (add vehicle to Vehical table) ──────
        public IActionResult RequestService()
        {
            if (!IsLoggedIn) return Guard();
            // Fetch customer email from DB and pass to view
            int id = HttpContext.Session.GetInt32("CustomerId") ?? 0;
            using var conn = _db.Open();
            using var cmd = new SqlCommand("SELECT Email FROM Customer WHERE Customer_id=@id", conn);
            cmd.Parameters.AddWithValue("@id", id);
            ViewBag.CustomerEmail = cmd.ExecuteScalar()?.ToString() ?? "";
            return View();
        }

        [HttpPost]
        public IActionResult RequestService(string NumberPlate, string Car_Name, string Car_Model,
            int? Year, string Owner_Name, string Owner_contact, string Owner_Email)
        {
            if (!IsLoggedIn) return Guard();
            try
            {
                using var conn = _db.Open();
                int newId = Convert.ToInt32(Scalar(conn, "SELECT ISNULL(MAX(Vech_id),0)+1 FROM Vehical"));
                // Store plate in Car_Name as prefix if RegId column is INT
                string carNameVal = string.IsNullOrEmpty(NumberPlate)
                    ? Car_Name ?? ""
                    : $"{Car_Name}|{NumberPlate}";
                object regIdVal = int.TryParse(NumberPlate, out int pn) ? (object)pn : DBNull.Value;
                try
                {
                    using var cmd = new SqlCommand(
                        "INSERT INTO Vehical(Vech_id,RegId,Car_Name,Car_Model,Owner_Name,Year,Owner_contact,status,Owner_Email) VALUES(@id,@r,@cn,@cm,@on,@y,@oc,@s,@oe)", conn);
                    cmd.Parameters.AddWithValue("@id", newId);
                    cmd.Parameters.AddWithValue("@r",  regIdVal);
                    cmd.Parameters.AddWithValue("@cn", carNameVal);
                    cmd.Parameters.AddWithValue("@cm", Car_Model ?? "");
                    cmd.Parameters.AddWithValue("@on", Owner_Name ?? "");
                    cmd.Parameters.AddWithValue("@y",  (object?)Year ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@oc", string.IsNullOrEmpty(Owner_contact) ? (object)DBNull.Value : int.Parse(Owner_contact));
                    cmd.Parameters.AddWithValue("@s",  "Pending");
                    cmd.Parameters.AddWithValue("@oe", Owner_Email ?? "");
                    cmd.ExecuteNonQuery();
                }
                catch
                {
                    using var cmd2 = new SqlCommand(
                        "INSERT INTO Vehical(Vech_id,RegId,Car_Name,Car_Model,Owner_Name,Year,Owner_contact,status) VALUES(@id,@r,@cn,@cm,@on,@y,@oc,@s)", conn);
                    cmd2.Parameters.AddWithValue("@id", newId);
                    cmd2.Parameters.AddWithValue("@r",  regIdVal);
                    cmd2.Parameters.AddWithValue("@cn", carNameVal);
                    cmd2.Parameters.AddWithValue("@cm", Car_Model ?? "");
                    cmd2.Parameters.AddWithValue("@on", Owner_Name ?? "");
                    cmd2.Parameters.AddWithValue("@y",  (object?)Year ?? DBNull.Value);
                    cmd2.Parameters.AddWithValue("@oc", string.IsNullOrEmpty(Owner_contact) ? (object)DBNull.Value : int.Parse(Owner_contact));
                    cmd2.Parameters.AddWithValue("@s",  "Pending");
                    cmd2.ExecuteNonQuery();
                }
                TempData["SvcSuccess"] = "Service request submitted successfully!";
            }
            catch (Exception ex)
            {
                TempData["SvcError"] = "Error: " + ex.Message;
            }
            return RedirectToAction("MyRequests");
        }

        // ── MY REQUESTS ─────────────────────────────────────────
        public IActionResult MyRequests()
        {
            if (!IsLoggedIn) return Guard();

            int custId   = HttpContext.Session.GetInt32("CustomerId") ?? 0;
            string email = "";
            string name  = HttpContext.Session.GetString("CustomerName") ?? "";

            // Get customer email for matching
            using var conn = _db.Open();
            using (var ec = new SqlCommand("SELECT Email FROM Customer WHERE Customer_id=@id", conn))
            {
                ec.Parameters.AddWithValue("@id", custId);
                email = ec.ExecuteScalar()?.ToString() ?? "";
            }

            // Fetch vehicles submitted by this customer (match by email OR name)
            var vehicles = new List<Vehicle>();
            try
            {
                string vSql = string.IsNullOrEmpty(email)
                    ? "SELECT Vech_id,RegId,Car_Name,Car_Model,Owner_Name,Year,Owner_contact,status FROM Vehical WHERE Owner_Name=@name ORDER BY Vech_id DESC"
                    : "SELECT Vech_id,RegId,Car_Name,Car_Model,Owner_Name,Year,Owner_contact,status FROM Vehical WHERE Owner_Name=@name OR Owner_Name=@email ORDER BY Vech_id DESC";

                // Try with Owner_Email column
                try
                {
                    using var vc = new SqlCommand(
                        "SELECT Vech_id,RegId,Car_Name,Car_Model,Owner_Name,Year,Owner_contact,status FROM Vehical WHERE Owner_Email=@email OR Owner_Name=@name ORDER BY Vech_id DESC", conn);
                    vc.Parameters.AddWithValue("@email", email);
                    vc.Parameters.AddWithValue("@name", name);
                    using var vr = vc.ExecuteReader();
                    while (vr.Read())
                    {
                        var rawCar = vr.IsDBNull(2) ? "" : vr.GetString(2);
                        var rawReg = vr.IsDBNull(1) ? null : vr.GetValue(1).ToString();
                        string plate = rawReg ?? "";
                        string carName = rawCar;
                        if (rawCar.Contains('|')) { var p = rawCar.Split('|',2); carName = p[0].Trim(); plate = p[1].Trim(); }
                        vehicles.Add(new Vehicle {
                            Vech_id = vr.GetInt32(0), RegId = plate, Car_Name = carName,
                            Car_Model = vr.IsDBNull(3)?"":vr.GetString(3),
                            Owner_Name = vr.IsDBNull(4)?"":vr.GetString(4),
                            Year = vr.IsDBNull(5)?null:vr.GetInt32(5),
                            Owner_contact = vr.IsDBNull(6)?null:vr.GetInt32(6),
                            Status = vr.IsDBNull(7)?"Pending":vr.GetString(7)
                        });
                    }
                }
                catch
                {
                    using var vc2 = new SqlCommand(
                        "SELECT Vech_id,RegId,Car_Name,Car_Model,Owner_Name,Year,Owner_contact,status FROM Vehical WHERE Owner_Name=@name ORDER BY Vech_id DESC", conn);
                    vc2.Parameters.AddWithValue("@name", name);
                    using var vr2 = vc2.ExecuteReader();
                    while (vr2.Read())
                    {
                        var rawCar = vr2.IsDBNull(2) ? "" : vr2.GetString(2);
                        var rawReg = vr2.IsDBNull(1) ? null : vr2.GetValue(1).ToString();
                        string plate = rawReg ?? "";
                        string carName = rawCar;
                        if (rawCar.Contains('|')) { var p = rawCar.Split('|',2); carName = p[0].Trim(); plate = p[1].Trim(); }
                        vehicles.Add(new Vehicle {
                            Vech_id = vr2.GetInt32(0), RegId = plate, Car_Name = carName,
                            Car_Model = vr2.IsDBNull(3)?"":vr2.GetString(3),
                            Owner_Name = vr2.IsDBNull(4)?"":vr2.GetString(4),
                            Year = vr2.IsDBNull(5)?null:vr2.GetInt32(5),
                            Owner_contact = vr2.IsDBNull(6)?null:vr2.GetInt32(6),
                            Status = vr2.IsDBNull(7)?"Pending":vr2.GetString(7)
                        });
                    }
                }
            }
            catch { }

            // Fetch job cards ONLY for this customer's vehicle IDs
            var jobs = new List<JobCard>();
            try
            {
                if (vehicles.Any())
                {
                    // Build IN clause from customer's vehicle IDs
                    var vIds = string.Join(",", vehicles.Select(v => v.Vech_id));
                    using var jc = new SqlCommand(
                        $@"SELECT j.J_id, j.Vech_id, j.Problem, j.Date_in, j.Date_out, j.Mech_Name, j.Status,
                                  v.Car_Name, v.Car_Model, v.RegId
                           FROM Job_Crad j
                           INNER JOIN Vehical v ON j.Vech_id = v.Vech_id
                           WHERE j.Vech_id IN ({vIds})
                           ORDER BY j.J_id DESC", conn);
                    using var jr = jc.ExecuteReader();
                    while (jr.Read())
                    {
                        var rawCar = jr.IsDBNull(7) ? "" : jr.GetString(7);
                        var rawReg = jr.IsDBNull(9) ? null : jr.GetValue(9).ToString();
                        string jPlate = rawReg ?? "";
                        string jCar   = rawCar;
                        if (rawCar.Contains('|')) { var p = rawCar.Split('|',2); jCar = p[0].Trim(); jPlate = p[1].Trim(); }
                        jobs.Add(new JobCard {
                            J_id      = jr.GetInt32(0),
                            Vech_id   = jr.IsDBNull(1) ? null : jr.GetInt32(1),
                            Problem   = jr.IsDBNull(2) ? "" : jr.GetString(2),
                            Date_in   = jr.IsDBNull(3) ? null : jr.GetDateTime(3),
                            Date_out  = jr.IsDBNull(4) ? null : jr.GetDateTime(4),
                            Mech_Name = jr.IsDBNull(5) ? "" : jr.GetString(5),
                            Status    = jr.IsDBNull(6) ? "Pending" : jr.GetString(6),
                            Car_Name  = $"{jCar} {jr.GetString(8)}".Trim(),
                            RegId     = jPlate
                        });
                    }
                }
            }
            catch { }

            ViewBag.Vehicles = vehicles;
            return View(jobs);
        }

        // ── LOGOUT ──────────────────────────────────────────────
        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            return RedirectToAction("Login", "Auth");
        }

        private static object? Scalar(SqlConnection conn, string sql)
        {
            using var cmd = new SqlCommand(sql, conn);
            return cmd.ExecuteScalar();
        }
    }
}
