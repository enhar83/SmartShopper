using Core_Layer.IServices;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace SmartShopper.Areas.Admin.Controllers
{
    [Area("Admin")]
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
                    return Json(new { success = true, message = "AI SubCategory Demand Forecasting completed successfully!" });
                }
                else
                {
                    return Json(new { success = false, message = "Not enough historical data to train the AI model. At least 20 order items required." });
                }
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "An error occurred during analysis: " + ex.Message });
            }
        }
    }
}