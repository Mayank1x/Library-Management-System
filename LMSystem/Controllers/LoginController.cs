using LMSystem.Models;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;

namespace LMSystem.Controllers
{
    public class LoginController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }

        public List<LoginModel> PutValue()
        {
            var users = new List<LoginModel>
            {
                new LoginModel { id = 1, username = "admin", password = "12345" },
                new LoginModel { id = 2, username = "mycodingproject", password = "myc546" },
                new LoginModel { id = 3, username = "my", password = "myc" },
            };
            return users;
        }

        [HttpPost]
        public async Task<IActionResult> Verify(LoginModel usr)
        {
            var u = PutValue();
            var ue = u.Where(x => x.username!.Equals(usr.username));
            var up = ue.Where(x => x.password!.Equals(usr.password));

            if (up.Count() == 1)
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
                ViewBag.message = "Login Failed";
                return View("Index");
            }
        }

        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("Index", "Login");
        }
    }
}
