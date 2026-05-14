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

        [HttpPost]
        public async Task<IActionResult> DeleteDiscount(Guid id)
        {
            try
            {
                await _discountService.TDeleteDiscountAsync(id);
                return Json(new { success = true, message = "The discount campaign has been successfully deleted!" });
            }
            catch (LogicException ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
            catch (Exception)
            {
                return Json(new { success = false, message = "An unexpected system error occurred while deleting." });
            }
        }

        public async Task<IActionResult> GetDiscountForEdit(Guid id)
        {
            try
            {
                var data = await _discountService.GetDiscountForUpdateAsync(id);
                return Json(new { success = true, data = data });
            }
            catch (LogicException ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
            catch (Exception)
            {
                return Json(new { success = false, message = "An unexpected system error occurred while fetching data." });
            }
        }

        [HttpPost]
        public async Task<IActionResult> UpdateDiscount(DiscountUpdateDto updateDto)
        {
            if (!ModelState.IsValid)
            {
                var validationErrors = ModelState.Values
                    .SelectMany(v => v.Errors)
                    .Select(e => e.ErrorMessage)
                    .ToList();

                return Json(new { success = false, message = "Please correct any errors in the form.", errors = validationErrors });
            }

            try
            {
                await _discountService.TUpdateDiscountAsync(updateDto);
                return Json(new { success = true, message = "The campaign has been successfully updated!" });
            }
            catch (LogicException ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
            catch (Exception)
            {
                return Json(new { success = false, message = "An unexpected system error occurred while updating." });
            }
        }

        [HttpPost]
        public async Task<IActionResult> AssignDiscount(AssignDiscountDto assignDto)
        {
            try
            {
                await _discountService.TAssignDiscountToUserAsync(assignDto);
                return Json(new { success = true, message = "The discount has been successfully applied!" });
            }
            catch (LogicException ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
            catch (Exception)
            {
                return Json(new { success = false, message = "A system error occurred during the assignment process." });
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetActiveDiscounts()
        {
            var discounts = await _discountService.TGetAllDiscountsAsync();
            var activeDiscounts = discounts.Where(x => !x.IsDeleted && x.EndDate > DateTime.Now).ToList();
            return Json(activeDiscounts);
        }
    }
}
