using Microsoft.AspNetCore.Mvc;

namespace LMSystem.Controllers
{
    public class ContactUsController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
