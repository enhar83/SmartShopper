using System.Security.Claims;
using Core_Layer.Dtos.OrderDtos;
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

        [HttpGet]
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
        public async Task<IActionResult> PlaceOrder(CheckoutPaymentDto paymentDto)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId == null)
                return RedirectToAction("Index", "Cart");

            var userIdString = Guid.Parse(userId);

            if (!ModelState.IsValid)
            {
                var summary = await _checkoutService.TGetCheckoutSummaryAsync(userIdString);
                ViewBag.Addresses = await _userAddressService.TGetUserAddressListCheckoutAsync(userIdString);
                ViewBag.Coupons = await _discountService.TGetAvailableDiscountsForCheckoutAsync(userIdString);

                return View("Index", summary);
            }

            var result = await _checkoutService.TPlaceOrderAsync(userIdString, paymentDto.AddressId, paymentDto.DiscountId);

            if (result)
            {
                if (paymentDto.DiscountId.HasValue && paymentDto.DiscountId.Value != Guid.Empty)
                    await _discountService.TMarkDiscountAsUsedAsync(userIdString, paymentDto.DiscountId.Value);

                TempData["SuccessMessage"] = "Your payment has been successfully processed and your order is received!";
                return RedirectToAction("OrderSuccess");
            }

            TempData["ErrorMessage"] = "An error occurred while creating the order. Please check the stock and try again.";

            var currentSummary = await _checkoutService.TGetCheckoutSummaryAsync(userIdString);
            ViewBag.Addresses = await _userAddressService.TGetUserAddressListCheckoutAsync(userIdString);
            ViewBag.Coupons = await _discountService.TGetAvailableDiscountsForCheckoutAsync(userIdString);

            return View("Index", currentSummary);
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