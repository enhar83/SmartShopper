using System.Security.Claims;
using Core_Layer.IRepositories;
using Core_Layer.IServices;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace SmartShopper.Controllers
{
    [Authorize(Roles = "Admin, Member")]
    public class DiscountController : Controller
    {
        private readonly IDiscountService _discountService;
        public DiscountController(IDiscountService discountService)
        {
            _discountService = discountService;
        }

        public async Task<IActionResult> Index()
        {
            var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userIdString == null)
                return NotFound("User not found.");

            var userId = Guid.Parse(userIdString);

            var myDiscounts = await _discountService.TGetUserSpecificDiscountsAsync(userId);
            return View(myDiscounts);
        }
    }
}
