using System.Security.Claims;
using Core_Layer.IServices;
using Microsoft.AspNetCore.Mvc;

namespace SmartShopper.Controllers
{
    public class CheckoutController : Controller
    {
        private readonly ICheckoutService _checkoutService;
        private readonly IUserAddressService _userAddressService;
        private readonly IDiscountService _discountService;

        public CheckoutController(
            ICheckoutService checkoutService,
            IUserAddressService userAddressService,
            IDiscountService discountService)
        {
            _checkoutService = checkoutService;
            _userAddressService = userAddressService;
            _discountService = discountService;
        }

        public async Task<IActionResult> Index()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId == null)
                return RedirectToAction("Index", "Cart");

            var userIdString = Guid.Parse(userId);

            var summary = await _checkoutService.TGetCheckoutSummaryAsync(userIdString);

            if (summary.Items == null || !summary.Items.Any())
            {
                return RedirectToAction("Index", "Cart");
            }

            var addresses = await _userAddressService.TGetUserAddressListCheckoutAsync(userIdString);
            ViewBag.Addresses = addresses;

            var coupons = await _discountService.TGetAvailableDiscountsForCheckoutAsync(userIdString);
            ViewBag.Coupons = coupons;

            return View(summary);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> PlaceOrder(Guid selectedAddressId, Guid? selectedDiscountId)
        {
            if (selectedAddressId == Guid.Empty)
            {
                TempData["ErrorMessage"] = "Please select a delivery address.";
                return RedirectToAction("Index");
            }

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId == null)
                return RedirectToAction("Index", "Cart");

            var userIdString = Guid.Parse(userId);

            var result = await _checkoutService.TPlaceOrderAsync(userIdString, selectedAddressId, selectedDiscountId);

            if (result)
            {
                if (selectedDiscountId.HasValue && selectedDiscountId.Value != Guid.Empty)
                    await _discountService.TMarkDiscountAsUsedAsync(userIdString, selectedDiscountId.Value);

                TempData["SuccessMessage"] = "Your order has been successfully received!";
                return RedirectToAction("OrderSuccess");
            }

            TempData["ErrorMessage"] = "An error occurred while creating the order. Please check the stock and try again.";
            return RedirectToAction("Index");
        }

        [HttpGet]
        public IActionResult OrderSuccess()
        {
            if (TempData["SuccessMessage"] == null)
                return RedirectToAction("Index", "Home");

            return View();
        }
    }
}