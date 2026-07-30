using Microsoft.AspNetCore.Mvc;

namespace LMSystem.Controllers
{
    public class AboutUsController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
