using System;
using System.Linq;
using System.Threading.Tasks;
using Core_Layer.IServices;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace SmartShopper.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class SubCategoryDemandForecastController : Controller
    {
        private readonly ISubCategoryDemandForecastService _forecastService;

        public SubCategoryDemandForecastController(ISubCategoryDemandForecastService forecastService)
        {
            _forecastService = forecastService;
        }

        public async Task<IActionResult> Index()
        {
            var data = await _forecastService.TGetAllForecastsAsync();
            ViewBag.Categories = data.Select(x => x.CategoryName).Distinct().OrderBy(x => x).ToList();

            var metrics = await _forecastService.TGetForecastMetricsAsync();
            ViewBag.ModelMetrics = metrics;

            return View(data);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RunForecast()
        {
            try
            {
                var success = await _forecastService.TTrainAndGenerateForecastsAsync();

                if (success)
                {
                    return Json(new { success = true, message = "AI Demand Forecasting has been successfully completed and metrics have been updated!" });
                }
                else
                {
                    return Json(new { success = false, message = "Insufficient historical order data was found to train the artificial intelligence." });
                }
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "An error occurred during the analysis: " + ex.Message });
            }
        }
    }
}