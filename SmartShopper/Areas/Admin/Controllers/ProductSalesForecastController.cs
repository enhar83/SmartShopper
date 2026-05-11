using Core_Layer.IServices;
using Microsoft.AspNetCore.Mvc;

namespace SmartShopper.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class ProductSalesForecastController : Controller
    {
        private readonly IProductSalesForecastService _productSalesForecastService;

        public ProductSalesForecastController(IProductSalesForecastService productSalesForecastService)
        {
            _productSalesForecastService = productSalesForecastService;
        }

        public async Task<IActionResult> Index()
        {
            var data = await _productSalesForecastService.TGetAllForecastsAsync();
            return View(data);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RunProductForecast()
        {
            try
            {
                var success = await _productSalesForecastService.TTrainAndGenerateForecastsAsync();

                if (success)
                {
                    return Json(new { success = true, message = "AI Inventory Forecasting completed successfully!" });
                }
                return Json(new { success = false, message = "Insufficient sales history to generate product forecasts." });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "An error occurred: " + ex.Message });
            }
        }
    }
}
