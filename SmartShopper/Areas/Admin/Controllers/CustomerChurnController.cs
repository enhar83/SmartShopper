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

        // SENIOR FIX: Sayfa açıldığında analiz yapmaz, sadece DB'deki son durumu gösterir. Sayfa fişek gibi açılır.
        public async Task<IActionResult> Index()
        {
            var data = await _churnService.TGetAllChurnResultsAsync();
            return View(data);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RunAnalysis()
        {
            try
            {
                // Analiz motoru (Machine Learning) sadece butona basıldığında çalışır!
                var results = await _churnService.TProcessAllCustomersChurnAsync();

                return Json(new { success = true, data = results, message = "Analysis completed successfully!" });
            }
            catch (Exception ex)
            {
                // Hata durumunda UI'a düzgün bir mesaj dönüyoruz
                return Json(new { success = false, message = "An error occurred during analysis: " + ex.Message });
            }
        }
    }
}