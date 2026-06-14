using Microsoft.AspNetCore.Mvc;

namespace SmartShopper.Controllers
{
    public class ErrorController : Controller
    {
        [Route("Error/{statusCode}")]
        public IActionResult HttpStatusCodeHandler(int statusCode)
        {
            var statusCodeResult = HttpContext.Features.Get<Microsoft.AspNetCore.Diagnostics.IStatusCodeReExecuteFeature>();

            switch (statusCode)
            {
                case 401:
                    ViewBag.ErrorMessage = "You must log in to view this page.";
                    return View("Page401");
                case 403:
                    ViewBag.ErrorMessage = "You do not have permission to access this page.";
                    return View("Page403"); 
                case 404:
                    ViewBag.ErrorMessage = "The page you are looking for could not be found.";
                    return View("Page404"); 
                default:
                    return View("DefaultError");
            }
        }
    }
}
