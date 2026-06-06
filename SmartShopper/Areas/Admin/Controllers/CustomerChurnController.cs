using Core_Layer.IServices;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;

namespace SmartShopper.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class CustomerChurnController : Controller
    {
        private readonly ICustomerChurnResultService _churnService;

        public CustomerChurnController(ICustomerChurnResultService churnService)
        {
            _churnService = churnService;
        }

        public async Task<IActionResult> Index()
        {
            var data = await _churnService.TGetAllChurnResultsAsync();
            var metrics = await _churnService.TGetChurnModelMetricsAsync();
            ViewBag.ModelMetrics = metrics;

            return View(data);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RunAnalysis()
        {
            try
            {
                var results = await _churnService.TProcessAllCustomersChurnAsync();

                return Json(new { success = true, data = results, message = "Analysis completed successfully!" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "An error occurred during analysis: " + ex.Message });
            }
        }
    }
}