using Microsoft.AspNetCore.Mvc;

namespace SmartShopper.Controllers
{
    public class AboutUsController : Controller
    {
        public IActionResult AboutUs()
        {
            return View();
        }
    }
}
