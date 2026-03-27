using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using VSMS.Models;

namespace VSMS.Controllers
{
    [IgnoreAntiforgeryToken]
    public class AdminController : Controller
    {
        private readonly DB _db;
        private readonly IConfiguration _config;
        public AdminController(DB db, IConfiguration config) { _db = db; _config = config; }

        private bool IsAdmin => HttpContext.Session.GetString("Role") == "Admin";
        private IActionResult Guard() => RedirectToAction("Login", "Auth");

        // Redirect old /Admin/Login bookmarks to unified login
        public IActionResult Login() => RedirectToAction("Login", "Auth");
        // ── DASHBOARD ───────────────────────────────────────────
        public IActionResult Dashboard()
        {
            if (!IsAdmin) return Guard();
            try
            {
                using var conn = _db.Open();
                ViewBag.CustomerCount = Scalar(conn, "SELECT COUNT(*) FROM Customer");
                ViewBag.VehicleCount  = Scalar(conn, "SELECT COUNT(*) FROM Vehical");
                ViewBag.JobCount      = Scalar(conn, "SELECT COUNT(*) FROM Job_Crad");
                ViewBag.Revenue       = Scalar(conn, "SELECT ISNULL(SUM(Total_cost),0) FROM Billing");
            }
            catch
            {
                ViewBag.CustomerCount = 0; ViewBag.VehicleCount = 0;
                ViewBag.JobCount = 0; ViewBag.Revenue = 0;
            }
            return View();
        }

        // ── CUSTOMERS ───────────────────────────────────────────
        public IActionResult Customers()
        {
            if (!IsAdmin) return Guard();
            var list = new List<Customer>();
            try
            {
                using var conn = _db.Open();
                using var cmd = new SqlCommand("SELECT Customer_id,Name,Email,Contact FROM Customer ORDER BY Customer_id DESC", conn);
                using var r = cmd.ExecuteReader();
                while (r.Read())
                    list.Add(new Customer {
                        Customer_id = r.GetInt32(0),
                        Name    = r.IsDBNull(1) ? "" : r.GetString(1),
                        Email   = r.IsDBNull(2) ? "" : r.GetString(2),
                        Contact = r.IsDBNull(3) ? "" : r.GetString(3)
                    });
            }
            catch (Exception ex) { ViewBag.DbError = ex.Message; }
            return View(list);
        }

        [HttpPost]
        public IActionResult DeleteCustomer(int id)
        {
            if (!IsAdmin) return Guard();
            using var conn = _db.Open();
            Exec(conn, "DELETE FROM Customer WHERE Customer_id=@id", ("@id", id));
            TempData["Toast"] = "Customer deleted.";
            return RedirectToAction("Customers");
        }

        // ── MECHANICS ───────────────────────────────────────────
        public IActionResult Mechanics()
        {
            if (!IsAdmin) return Guard();
            var list = new List<Mechanic>();
            try
            {
                using var conn = _db.Open();
                using var cmd = new SqlCommand("SELECT Mech_id,Mech_Name,Mech_Contact,Status FROM Mechanic ORDER BY Mech_id", conn);
                using var r = cmd.ExecuteReader();
                while (r.Read())
                    list.Add(new Mechanic {
                        Mech_id      = r.GetInt32(0),
                        Mech_Name    = r.IsDBNull(1) ? "" : r.GetString(1),
                        Mech_Contact = r.IsDBNull(2) ? "" : r.GetString(2),
                        Status       = r.IsDBNull(3) ? "Available" : r.GetString(3)
                    });
            }
            catch (Exception ex) { ViewBag.DbError = ex.Message; }
            return View(list);
        }

        [HttpPost]
        public IActionResult AddMechanic(Mechanic m)
        {
            if (!IsAdmin) return Guard();
            try
            {
                using var conn = _db.Open();
                int newId = Convert.ToInt32(Scalar(conn, "SELECT ISNULL(MAX(Mech_id),0)+1 FROM Mechanic"));
                Exec(conn, "INSERT INTO Mechanic(Mech_id,Mech_Name,Mech_Contact,Status) VALUES(@id,@n,@c,@s)",
                    ("@id", newId), ("@n", m.Mech_Name ?? ""), ("@c", m.Mech_Contact ?? ""), ("@s", m.Status ?? "Available"));
                TempData["Toast"] = "Mechanic added.";
            }
            catch (Exception ex) { TempData["Toast"] = "Error: " + ex.Message; }
            return RedirectToAction("Mechanics");
        }

        [HttpPost]
        public IActionResult DeleteMechanic(int id)
        {
            if (!IsAdmin) return Guard();
            using var conn = _db.Open();
            Exec(conn, "DELETE FROM Mechanic WHERE Mech_id=@id", ("@id", id));
            TempData["Toast"] = "Mechanic deleted.";
            return RedirectToAction("Mechanics");
        }

        // ── VEHICLES ────────────────────────────────────────────
        public IActionResult Vehicles()
        {
            if (!IsAdmin) return Guard();
            var list = new List<Vehicle>();
            try
            {
                using var conn = _db.Open();
                // Try with Owner_Email column first
                string sql = "SELECT Vech_id,RegId,Car_Name,Car_Model,Owner_Name,Year,Owner_contact,status,Owner_Email FROM Vehical ORDER BY Vech_id DESC";
                bool hasEmailCol = true;
                try
                {
                    using var test = new SqlCommand("SELECT TOP 0 Owner_Email FROM Vehical", conn);
                    test.ExecuteNonQuery();
                }
                catch { hasEmailCol = false; sql = "SELECT Vech_id,RegId,Car_Name,Car_Model,Owner_Name,Year,Owner_contact,status FROM Vehical ORDER BY Vech_id DESC"; }

                using var cmd = new SqlCommand(sql, conn);
                using var r = cmd.ExecuteReader();
                while (r.Read())
                {
                    var rawCarName = r.IsDBNull(2) ? "" : r.GetString(2);
                    var rawRegId   = r.IsDBNull(1) ? null : r.GetValue(1).ToString();
                    // Extract plate from Car_Name if stored as "Toyota|MH12AB1234"
                    string plate = rawRegId ?? "";
                    string carName = rawCarName;
                    if (rawCarName.Contains('|'))
                    {
                        var parts = rawCarName.Split('|', 2);
                        carName = parts[0].Trim();
                        plate   = parts[1].Trim();
                    }
                    list.Add(new Vehicle {
                        Vech_id       = r.GetInt32(0),
                        RegId         = plate,
                        Car_Name      = carName,
                        Car_Model     = r.IsDBNull(3) ? "" : r.GetString(3),
                        Owner_Name    = r.IsDBNull(4) ? "" : r.GetString(4),
                        Year          = r.IsDBNull(5) ? null : r.GetInt32(5),
                        Owner_contact = r.IsDBNull(6) ? null : r.GetInt32(6),
                        Status        = r.IsDBNull(7) ? "Pending" : r.GetString(7),
                        Owner_Email   = hasEmailCol && !r.IsDBNull(8) ? r.GetString(8) : ""
                    });
                }
            }
            catch (Exception ex) { ViewBag.DbError = ex.Message; }
            return View(list);
        }

        [HttpPost]
        public IActionResult AddVehicle(Vehicle v)
        {
            if (!IsAdmin) return Guard();
            using var conn = _db.Open();
            int newId = Convert.ToInt32(Scalar(conn, "SELECT ISNULL(MAX(Vech_id),0)+1 FROM Vehical"));
            // Store plate in Car_Name as prefix if RegId is not numeric
            string carName = v.Car_Name ?? "";
            object regIdVal = DBNull.Value;
            if (!string.IsNullOrEmpty(v.RegId))
            {
                if (int.TryParse(v.RegId, out int rid))
                    regIdVal = rid;
                else
                    carName = $"{carName}|{v.RegId}"; // store plate in Car_Name
            }
            try
            {
                Exec(conn, "INSERT INTO Vehical(Vech_id,RegId,Car_Name,Car_Model,Owner_Name,Year,Owner_contact,status,Owner_Email) VALUES(@id,@r,@cn,@cm,@on,@y,@oc,@s,@oe)",
                    ("@id", newId), ("@r", regIdVal),
                    ("@cn", carName), ("@cm", v.Car_Model ?? ""),
                    ("@on", v.Owner_Name ?? ""), ("@y", (object?)v.Year ?? DBNull.Value),
                    ("@oc", (object?)v.Owner_contact ?? DBNull.Value),
                    ("@s", "Pending"), ("@oe", v.Owner_Email ?? ""));
            }
            catch
            {
                Exec(conn, "INSERT INTO Vehical(Vech_id,RegId,Car_Name,Car_Model,Owner_Name,Year,Owner_contact,status) VALUES(@id,@r,@cn,@cm,@on,@y,@oc,@s)",
                    ("@id", newId), ("@r", regIdVal),
                    ("@cn", carName), ("@cm", v.Car_Model ?? ""),
                    ("@on", v.Owner_Name ?? ""), ("@y", (object?)v.Year ?? DBNull.Value),
                    ("@oc", (object?)v.Owner_contact ?? DBNull.Value),
                    ("@s", "Pending"));
            }
            TempData["Toast"] = "Vehicle added.";
            return RedirectToAction("Vehicles");
        }

        [HttpPost]
        public IActionResult UpdateVehicleStatus(int id, string status)
        {
            if (!IsAdmin) return Guard();
            using var conn = _db.Open();
            Exec(conn, "UPDATE Vehical SET status=@s WHERE Vech_id=@id", ("@s", status), ("@id", id));
            TempData["Toast"] = $"Vehicle status updated to {status}.";
            return RedirectToAction("Vehicles");
        }

        [HttpPost]
        public IActionResult SendVehicleEmail(int id)
        {
            if (!IsAdmin) return Guard();
            try
            {
                // Fetch vehicle + customer email
                Vehicle? v = null;
                string? customerEmail = null;
                using var conn = _db.Open();
                using (var cmd = new SqlCommand(
                    "SELECT Vech_id,RegId,Car_Name,Car_Model,Owner_Name,Year,Owner_contact,status,ISNULL(TRY_CAST(Owner_Email AS VARCHAR(100)),'') FROM Vehical WHERE Vech_id=@id", conn))
                {
                    cmd.Parameters.AddWithValue("@id", id);
                    SqlDataReader? r = null;
                    try { r = cmd.ExecuteReader(); }
                    catch
                    {
                        // Owner_Email column missing, query without it
                        using var cmd2 = new SqlCommand("SELECT Vech_id,RegId,Car_Name,Car_Model,Owner_Name,Year,Owner_contact,status FROM Vehical WHERE Vech_id=@id2", conn);
                        cmd2.Parameters.AddWithValue("@id2", id);
                        using var r2 = cmd2.ExecuteReader();
                        if (r2.Read())
                            v = new Vehicle {
                                Vech_id = r2.GetInt32(0), RegId = r2.IsDBNull(1) ? "" : r2.GetValue(1).ToString(),
                                Car_Name = r2.IsDBNull(2) ? "" : r2.GetString(2), Car_Model = r2.IsDBNull(3) ? "" : r2.GetString(3),
                                Owner_Name = r2.IsDBNull(4) ? "" : r2.GetString(4), Year = r2.IsDBNull(5) ? null : r2.GetInt32(5),
                                Owner_contact = r2.IsDBNull(6) ? null : r2.GetInt32(6), Status = r2.IsDBNull(7) ? "Pending" : r2.GetString(7),
                                Owner_Email = ""
                            };
                    }
                    if (r != null)
                    {
                        using (r)
                        if (r.Read())
                            v = new Vehicle {
                                Vech_id       = r.GetInt32(0),
                                RegId         = r.IsDBNull(1) ? "" : r.GetValue(1).ToString(),
                                Car_Name      = r.IsDBNull(2) ? "" : r.GetString(2),
                                Car_Model     = r.IsDBNull(3) ? "" : r.GetString(3),
                                Owner_Name    = r.IsDBNull(4) ? "" : r.GetString(4),
                                Year          = r.IsDBNull(5) ? null : r.GetInt32(5),
                                Owner_contact = r.IsDBNull(6) ? null : r.GetInt32(6),
                                Status        = r.IsDBNull(7) ? "Pending" : r.GetString(7),
                                Owner_Email   = r.IsDBNull(8) ? "" : r.GetString(8)
                            };
                    }
                    customerEmail = v?.Owner_Email;
                }

                if (v == null)
                {
                    TempData["Toast"] = "Vehicle not found.";
                    return RedirectToAction("Vehicles");
                }

                if (string.IsNullOrEmpty(customerEmail))
                {
                    TempData["Toast"] = "No email found for this vehicle owner.";
                    return RedirectToAction("Vehicles");
                }

                // Build email
                var cfg      = _config.GetSection("EmailSettings");
                var from     = cfg["SenderEmail"]!;
                var fromName = cfg["SenderName"] ?? "VSMS Admin";
                var appPass  = cfg["AppPassword"]!;
                var host     = cfg["SmtpHost"] ?? "smtp.gmail.com";
                var port     = int.Parse(cfg["SmtpPort"] ?? "587");

                var statusColor = v.Status == "Approved" ? "#16a34a" : v.Status == "Declined" ? "#dc2626" : "#d97706";
                var statusIcon  = v.Status == "Approved" ? "✅" : v.Status == "Declined" ? "❌" : "⏳";

                var htmlBody = $"""
                    <div style="font-family:Inter,Arial,sans-serif;max-width:600px;margin:0 auto;background:#f8fafc;padding:0;border-radius:16px;overflow:hidden;">
                        <div style="background:linear-gradient(135deg,#4338ca,#0ea5e9);padding:36px 40px;text-align:center;">
                            <h1 style="color:white;margin:0;font-size:1.8rem;font-weight:800;">🚗 VSMS Pro</h1>
                            <p style="color:rgba(255,255,255,0.8);margin:8px 0 0;">Vehicle Service Management System</p>
                        </div>
                        <div style="background:white;padding:40px;">
                            <h2 style="color:#0f172a;font-size:1.4rem;margin-bottom:8px;">Dear {v.Owner_Name},</h2>
                            <p style="color:#475569;margin-bottom:24px;">Your vehicle service request has been reviewed. Here is the current status:</p>

                            <div style="background:#f1f5f9;border-radius:12px;padding:24px;margin-bottom:24px;">
                                <table style="width:100%;border-collapse:collapse;">
                                    <tr><td style="padding:8px 0;color:#64748b;font-weight:600;">Number Plate</td><td style="padding:8px 0;color:#0f172a;font-weight:700;">{v.RegId}</td></tr>
                                    <tr><td style="padding:8px 0;color:#64748b;font-weight:600;">Car</td><td style="padding:8px 0;color:#0f172a;font-weight:700;">{v.Car_Name} {v.Car_Model}</td></tr>
                                    <tr><td style="padding:8px 0;color:#64748b;font-weight:600;">Year</td><td style="padding:8px 0;color:#0f172a;font-weight:700;">{v.Year}</td></tr>
                                    <tr><td style="padding:8px 0;color:#64748b;font-weight:600;">Contact</td><td style="padding:8px 0;color:#0f172a;font-weight:700;">{v.Owner_contact}</td></tr>
                                    <tr><td style="padding:8px 0;color:#64748b;font-weight:600;">Status</td>
                                        <td style="padding:8px 0;">
                                            <span style="background:{statusColor};color:white;padding:4px 16px;border-radius:20px;font-weight:700;font-size:0.9rem;">
                                                {statusIcon} {v.Status}
                                            </span>
                                        </td>
                                    </tr>
                                </table>
                            </div>

                            <p style="color:#475569;margin-bottom:0;">
                                {(v.Status == "Approved"
                                    ? "Your vehicle has been <strong>approved</strong>. Our team will contact you shortly to schedule the service."
                                    : v.Status == "Declined"
                                        ? "Unfortunately your vehicle request has been <strong>declined</strong>. Please contact us for more information."
                                        : "Your vehicle request is currently <strong>under review</strong>. We will notify you once it is processed.")}
                            </p>
                        </div>
                        <div style="background:#f1f5f9;padding:24px 40px;text-align:center;">
                            <p style="color:#94a3b8;font-size:0.85rem;margin:0;">© {DateTime.Now.Year} VSMS Pro — Vehicle Service Management System</p>
                        </div>
                    </div>
                """;

                using var message = new MimeKit.MimeMessage();
                message.From.Add(new MimeKit.MailboxAddress(fromName, from));
                message.To.Add(new MimeKit.MailboxAddress(v.Owner_Name, customerEmail));
                message.Subject = $"VSMS — Your Vehicle Status: {v.Status} | {v.Car_Name} {v.Car_Model}";
                message.Body = new MimeKit.TextPart(MimeKit.Text.TextFormat.Html) { Text = htmlBody };

                using var smtp = new MailKit.Net.Smtp.SmtpClient();
                smtp.Connect(host, port, MailKit.Security.SecureSocketOptions.StartTls);
                smtp.Authenticate(from, appPass);
                smtp.Send(message);
                smtp.Disconnect(true);

                TempData["Toast"] = $"Email sent to {customerEmail} successfully!";
            }
            catch (Exception ex)
            {
                TempData["Toast"] = $"Email failed: {ex.Message}";
            }
            return RedirectToAction("Vehicles");
        }

        [HttpPost]
        public IActionResult DeleteVehicle(int id)
        {
            if (!IsAdmin) return Guard();
            using var conn = _db.Open();
            Exec(conn, "DELETE FROM Vehical WHERE Vech_id=@id", ("@id", id));
            TempData["Toast"] = "Vehicle deleted.";
            return RedirectToAction("Vehicles");
        }

        // ── JOB CARDS ───────────────────────────────────────────
        public IActionResult JobCards()
        {
            if (!IsAdmin) return Guard();
            var jobs = new List<JobCard>();
            var vehicles = new List<Vehicle>();
            var mechanics = new List<Mechanic>();
            try
            {
                using var conn = _db.Open();
                using (var cmd = new SqlCommand("SELECT J_id,Vech_id,Problem,Date_in,Date_out,Mech_Name,Status FROM Job_Crad ORDER BY J_id DESC", conn))
                using (var r = cmd.ExecuteReader())
                    while (r.Read())
                        jobs.Add(new JobCard {
                            J_id      = r.GetInt32(0),
                            Vech_id   = r.IsDBNull(1) ? null : r.GetInt32(1),
                            Problem   = r.IsDBNull(2) ? "" : r.GetString(2),
                            Date_in   = r.IsDBNull(3) ? null : r.GetDateTime(3),
                            Date_out  = r.IsDBNull(4) ? null : r.GetDateTime(4),
                            Mech_Name = r.IsDBNull(5) ? "" : r.GetString(5),
                            Status    = r.IsDBNull(6) ? "Pending" : r.GetString(6)
                        });

                using (var cmd2 = new SqlCommand("SELECT Vech_id,Car_Name,Car_Model FROM Vehical ORDER BY Vech_id", conn))
                using (var r2 = cmd2.ExecuteReader())
                    while (r2.Read())
                        vehicles.Add(new Vehicle {
                            Vech_id   = r2.GetInt32(0),
                            Car_Name  = r2.IsDBNull(1) ? "" : r2.GetString(1),
                            Car_Model = r2.IsDBNull(2) ? "" : r2.GetString(2)
                        });

                try
                {
                    using var cmd3 = new SqlCommand("SELECT Mech_id,Mech_Name FROM Mechanic ORDER BY Mech_id", conn);
                    using var r3 = cmd3.ExecuteReader();
                    while (r3.Read())
                        mechanics.Add(new Mechanic {
                            Mech_id   = r3.GetInt32(0),
                            Mech_Name = r3.IsDBNull(1) ? "" : r3.GetString(1)
                        });
                }
                catch { /* Mechanic table may not exist yet */ }
            }
            catch (Exception ex) { ViewBag.DbError = ex.Message; }

            ViewBag.Vehicles  = vehicles;
            ViewBag.Mechanics = mechanics;
            return View(jobs);
        }

        [HttpPost]
        public IActionResult AddJobCard(JobCard j)
        {
            if (!IsAdmin) return Guard();
            using var conn = _db.Open();
            int newId = Convert.ToInt32(Scalar(conn, "SELECT ISNULL(MAX(J_id),0)+1 FROM Job_Crad"));
            Exec(conn, "INSERT INTO Job_Crad(J_id,Vech_id,Problem,Date_in,Date_out,Mech_Name,Status) VALUES(@id,@v,@p,@di,@do,@mn,@s)",
                ("@id", newId), ("@v", (object?)j.Vech_id ?? DBNull.Value),
                ("@p", j.Problem ?? ""), ("@di", (object?)j.Date_in ?? DBNull.Value),
                ("@do", (object?)j.Date_out ?? DBNull.Value),
                ("@mn", j.Mech_Name ?? ""), ("@s", j.Status ?? "Pending"));
            TempData["Toast"] = "Job card created.";
            return RedirectToAction("JobCards");
        }

        [HttpPost]
        public IActionResult DeleteJobCard(int id)
        {
            if (!IsAdmin) return Guard();
            using var conn = _db.Open();
            Exec(conn, "DELETE FROM Job_Crad WHERE J_id=@id", ("@id", id));
            TempData["Toast"] = "Job card deleted.";
            return RedirectToAction("JobCards");
        }

        [HttpPost]
        public IActionResult UpdateJobStatus(int id, string status)
        {
            if (!IsAdmin) return Guard();
            using var conn = _db.Open();
            Exec(conn, "UPDATE Job_Crad SET Status=@s WHERE J_id=@id", ("@s", status), ("@id", id));
            TempData["Toast"] = $"Job #{id} status updated to {status}.";
            return RedirectToAction("JobCards");
        }

        // ── BILLING ─────────────────────────────────────────────
        public IActionResult Billing()
        {
            if (!IsAdmin) return Guard();
            var list = new List<Billing>();
            var jobIds = new List<int>();
            try
            {
                using var conn = _db.Open();
                using (var cmd = new SqlCommand("SELECT B_id,J_id,Part_cost,Gst,Total_cost,Status,Payment_type FROM Billing ORDER BY B_id DESC", conn))
                using (var r = cmd.ExecuteReader())
                    while (r.Read())
                        list.Add(new Billing {
                            B_id         = r.GetInt32(0),
                            J_id         = r.IsDBNull(1) ? null : r.GetInt32(1),
                            Part_cost    = r.IsDBNull(2) ? null : r.GetInt32(2),
                            Gst          = r.IsDBNull(3) ? null : r.GetInt32(3),
                            Total_cost   = r.IsDBNull(4) ? null : r.GetInt32(4),
                            Status       = r.IsDBNull(5) ? "" : r.GetString(5),
                            Payment_type = r.IsDBNull(6) ? "" : r.GetString(6)
                        });

                using (var cmd2 = new SqlCommand("SELECT J_id FROM Job_Crad ORDER BY J_id", conn))
                using (var r2 = cmd2.ExecuteReader())
                    while (r2.Read()) jobIds.Add(r2.GetInt32(0));
            }
            catch (Exception ex) { ViewBag.DbError = ex.Message; }

            ViewBag.JobIds = jobIds;
            return View(list);
        }

        [HttpPost]
        public IActionResult AddBilling(Billing b)
        {
            if (!IsAdmin) return Guard();
            using var conn = _db.Open();
            int newId = Convert.ToInt32(Scalar(conn, "SELECT ISNULL(MAX(B_id),0)+1 FROM Billing"));
            Exec(conn, "INSERT INTO Billing(B_id,J_id,Part_cost,Gst,Total_cost,Status,Payment_type) VALUES(@id,@j,@pc,@g,@tc,@s,@pt)",
                ("@id", newId), ("@j", (object?)b.J_id ?? DBNull.Value),
                ("@pc", (object?)b.Part_cost ?? DBNull.Value),
                ("@g",  (object?)b.Gst ?? DBNull.Value),
                ("@tc", (object?)b.Total_cost ?? DBNull.Value),
                ("@s",  b.Status ?? "Pending"), ("@pt", b.Payment_type ?? "Cash"));
            TempData["Toast"] = "Invoice saved.";
            return RedirectToAction("Billing");
        }

        [HttpPost]
        public IActionResult DeleteBilling(int id)
        {
            if (!IsAdmin) return Guard();
            using var conn = _db.Open();
            Exec(conn, "DELETE FROM Billing WHERE B_id=@id", ("@id", id));
            TempData["Toast"] = "Billing record deleted.";
            return RedirectToAction("Billing");
        }

        // ── MISC ────────────────────────────────────────────────
        public IActionResult Profile()
        {
            if (!IsAdmin) return Guard();
            return View();
        }

        public IActionResult Reports()
        {
            if (!IsAdmin) return Guard();
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error() =>
            View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });

        // ── HELPERS ─────────────────────────────────────────────
        private static object? Scalar(SqlConnection conn, string sql)
        {
            using var cmd = new SqlCommand(sql, conn);
            return cmd.ExecuteScalar();
        }

        private static void Exec(SqlConnection conn, string sql, params (string, object?)[] p)
        {
            using var cmd = new SqlCommand(sql, conn);
            foreach (var (k, v) in p) cmd.Parameters.AddWithValue(k, v ?? DBNull.Value);
            cmd.ExecuteNonQuery();
        }
    }
}
