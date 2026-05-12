using Core_Layer.IServices;
using Microsoft.AspNetCore.Mvc;

namespace SmartShopper.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class OrderAnomalyController : Controller
    {
        private readonly IOrderAnomalyService _orderAnomalyService;

        public OrderAnomalyController(IOrderAnomalyService orderAnomalyService)
        {
            _orderAnomalyService = orderAnomalyService;
        }

        public async Task<IActionResult> Index()
        {
            var data = await _orderAnomalyService.TGetAllAnomaliesAsync();
            return View(data);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RunDetection()
        {
            try
            {
                var result = await _orderAnomalyService.TRunAnomalyDetectionAsync();
                if (result)
                {
                    return Json(new { success = true, message = "AI Security Scan completed. New anomalies flagged!" });
                }
                return Json(new { success = false, message = "Not enough data for scanning." });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }
    }
}
