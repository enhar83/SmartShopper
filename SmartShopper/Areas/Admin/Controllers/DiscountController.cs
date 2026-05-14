using Core_Layer.Dtos.DiscountDtos;
using Core_Layer.Exceptions;
using Core_Layer.IServices;
using Microsoft.AspNetCore.Mvc;

namespace SmartShopper.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class DiscountController : Controller
    {
        private readonly IDiscountService _discountService;

        public DiscountController(IDiscountService discountService)
        {
            _discountService = discountService;
        }

        [HttpGet]
        public async Task<IActionResult> DiscountList()
        {
            var discounts = await _discountService.TGetAllDiscountsAsync();
            return View(discounts);
        }

        [HttpPost]
        public async Task<IActionResult> CreateDiscount(DiscountCreateDto createDto)
        {
            if (!ModelState.IsValid)
            {
                var validationErrors = ModelState.Values
                    .SelectMany(v => v.Errors)
                    .Select(e => e.ErrorMessage)
                    .ToList();

                return Json(new
                {
                    success = false,
                    message = "Please correct any errors in the form.",
                    errors = validationErrors
                });
            }

            try
            {
                await _discountService.TCreateDiscountAsync(createDto);
                return Json(new
                {
                    success = true,
                    message = "The discount campaign has been successfully created!"
                });
            }
            catch (LogicException ex)
            {
                return Json(new
                {
                    success = false,
                    message = ex.Message 
                });
            }
            catch (Exception ex)
            {
                string errorMessage = ex.Message;
                if (ex.InnerException != null)
                {
                    errorMessage += " | Detay: " + ex.InnerException.Message;
                }

                return Json(new
                {
                    success = false,
                    message = "HATA: " + errorMessage
                });
            }
        }
    }
}
