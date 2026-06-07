using Core_Layer.IServices;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;

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

        public async Task<IActionResult> Index()
        {
            // SENIOR DOKUNUŞU: Hata metriklerini arayüze taşıyoruz
            var metrics = await _salesForecastingService.TGetForecastMetricsAsync();
            ViewBag.ModelMetrics = metrics;

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

        // SENIOR DOKUNUŞU: Modeli arayüzden asenkron eğitmek için yeni endpoint
        [HttpPost]
        public async Task<IActionResult> TrainModel()
        {
            try
            {
                await _salesForecastingService.TTrainForecastModelAsync();
                return Json(new { success = true, message = "AI Model başarıyla eğitildi ve metrikler güncellendi!" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }
    }
}