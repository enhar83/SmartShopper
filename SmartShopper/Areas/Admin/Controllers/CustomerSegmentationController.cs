using Core_Layer.IServices;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace SmartShopper.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class CustomerSegmentationController : Controller
    {
        private readonly ICustomerSegmentationService _customerSegmentationService;

        public CustomerSegmentationController(ICustomerSegmentationService customerSegmentationService)
        {
            _customerSegmentationService = customerSegmentationService;
        }

        public async Task<IActionResult> Index()
        {
            var results = await _customerSegmentationService.TGetSegmentationResultsAsync();
            var metrics = await _customerSegmentationService.TGetModelMetricsAsync();
            ViewBag.ModelMetrics = metrics;
            return View(results);
        }

        [HttpPost]
        public async Task<IActionResult> TrainModel()
        {
            var result = await _customerSegmentationService.TTrainModelAsync();
            if (result) return Json(new { success = true });
            return Json(new { success = false, message = "Yeterli veri yok." });
        }

        [HttpPost]
        public async Task<IActionResult> ProcessBatch()
        {
            try
            {
                await _customerSegmentationService.TProcessBatchSegmentationAsync();
                return Json(new { success = true });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        public async Task<IActionResult> GetUserInsight(Guid userId)
        {
            var insight = await _customerSegmentationService.TGetUserSegmentAsync(userId);
            return Json(insight);
        }
    }
}