using Core_Layer.IServices;
using Microsoft.AspNetCore.Mvc;

namespace SmartShopper.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class SalesForecastingController : Controller
    {
        private readonly ISalesForecastingService _salesForecastingService;

        public SalesForecastingController(ISalesForecastingService salesForecastingService)
        {
            _salesForecastingService = salesForecastingService;
        }

        public IActionResult Index()
        {
            return View();
        }

        [HttpGet]
        public async Task<JsonResult> GetForecastData(int months = 6)
        {
            try
            {
                var result = await _salesForecastingService.TGetSalesForecastAsync(months);

                return Json(new { success = true, data = result });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }
    }
}
