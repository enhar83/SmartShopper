using Microsoft.AspNetCore.Mvc;

namespace SmartShopper.Controllers
{
    public class ProductController : Controller
    {
        public IActionResult ProductList()
        {
            return View();
        }
    }
}
