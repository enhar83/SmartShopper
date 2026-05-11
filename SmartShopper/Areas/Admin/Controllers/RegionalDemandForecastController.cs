using Core_Layer.IServices;
using Microsoft.AspNetCore.Mvc;

namespace SmartShopper.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class RegionalDemandForecastController : Controller
    {
        private readonly IRegionalDemandForecastService _regionalDemandForecastService;

        public RegionalDemandForecastController(IRegionalDemandForecastService regionalDemandForecastService)
        {
            _regionalDemandForecastService = regionalDemandForecastService;
        }

        public async Task<IActionResult> Index()
        {
            var data = await _regionalDemandForecastService.TGetAllForecastsAsync();
            return View(data);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RunForecast()
        {
            try
            {
                var success = await _regionalDemandForecastService.TTrainAndGenerateForecastsAsync();

                if (success)
                {
                    return Json(new { success = true, message = "AI Demand Forecasting completed successfully!" });
                }
                else
                {
                    return Json(new { success = false, message = "Not enough historical data to train the AI model." });
                }
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "An error occurred during analysis: " + ex.Message });
            }
        }
    }
}
