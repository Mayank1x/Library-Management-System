using LMSystem.Models;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using MySqlConnector;

namespace LMSystem.Controllers
{
    public class LoginController : Controller
    {
        private readonly IConfiguration _config;

        public LoginController(IConfiguration config)
        {
            _config = config;
        }

        public IActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Verify(LoginModel usr)
        {
            using var con = new MySqlConnection(_config.GetConnectionString("DefaultConnection"));
            var cmd = new MySqlCommand("SELECT COUNT(*) FROM logintab WHERE Username=@u AND Password=@p", con);
            cmd.Parameters.AddWithValue("@u", usr.username);
            cmd.Parameters.AddWithValue("@p", usr.password);
            
            con.Open();
            int count = Convert.ToInt32(cmd.ExecuteScalar());

            if (count == 1)
            {
                var claims = new List<Claim>
                {
                    new Claim(ClaimTypes.Name, usr.username!)
                };

                var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);

                await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(claimsIdentity));

                TempData["message"] = "Login Success";
                return RedirectToAction("Index", "Dashboard");
            }
            else
            {
                ViewBag.message = "Login Failed: Invalid Username or Password.";
                return View("Index");
            }
        }

        // GET: Register
        public IActionResult Register()
        {
            return View();
        }

        // POST: Register
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Register(RegisterModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            using var con = new MySqlConnection(_config.GetConnectionString("DefaultConnection"));
            con.Open();

            // Check if username already exists
            var checkCmd = new MySqlCommand("SELECT COUNT(*) FROM logintab WHERE Username=@u", con);
            checkCmd.Parameters.AddWithValue("@u", model.Username);
            int count = Convert.ToInt32(checkCmd.ExecuteScalar());

            if (count > 0)
            {
                ModelState.AddModelError("Username", "Username is already taken.");
                return View(model);
            }

            // Insert new user
            var insertCmd = new MySqlCommand("INSERT INTO logintab (Username, Password) VALUES (@u, @p)", con);
            insertCmd.Parameters.AddWithValue("@u", model.Username);
            insertCmd.Parameters.AddWithValue("@p", model.Password); // Note: In production, passwords should be hashed!
            insertCmd.ExecuteNonQuery();

            TempData["SuccessMessage"] = "Registration successful! You can now log in.";
            return RedirectToAction("Index");
        }

        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("Index", "Login");
        }
    }
}
