using System;
using System.Threading.Tasks;
using Core_Layer.IServices;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace SmartShopper.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
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

            var metrics = await _productSalesForecastService.TGetForecastMetricsAsync();
            ViewBag.ModelMetrics = metrics;

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
                    return Json(new { success = true, message = "AI Product Inventory Forecasting completed successfully!" });
                }
                return Json(new { success = false, message = "Insufficient sales history to generate product forecasts. (Min 20 items required)" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "An error occurred: " + ex.Message });
            }
        }
    }
}