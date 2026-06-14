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
    public class CustomerChurnController : Controller
    {
        private readonly ICustomerChurnResultService _churnService;
        private readonly IDiscountService _discountService; 

        public CustomerChurnController(ICustomerChurnResultService churnService, IDiscountService discountService)
        {
            _churnService = churnService;
            _discountService = discountService;
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

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AssignBulkDiscount(decimal minProbability, decimal maxProbability, Guid discountId)
        {
            try
            {
                if (discountId == Guid.Empty)
                    return Json(new { success = false, message = "Invalid discount campaign selected." });

                if (minProbability < 0 || maxProbability > 100 || minProbability > maxProbability)
                    return Json(new { success = false, message = "Invalid churn probability range." });

                var userIds = await _churnService.TGetUsersByChurnProbabilityRangeAsync(minProbability, maxProbability);

                if (userIds == null || !userIds.Any())
                    return Json(new { success = false, message = "No customers found within this probability range." });

                await _discountService.TAssignDiscountToMultipleUsersAsync(discountId, userIds);

                return Json(new { success = true, message = $"Successfully assigned discount to {userIds.Count} customers. (Users who already had the discount were skipped)." });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "An error occurred during bulk assignment: " + ex.Message });
            }
        }
    }
}