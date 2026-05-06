using System.Security.Claims;
using Core_Layer.IServices;
using Microsoft.AspNetCore.Mvc;

namespace SmartShopper.Controllers
{
    public class CheckoutController : Controller
    {
        private readonly ICheckoutService _checkoutService;
        private readonly IUserAddressService _userAddressService;

        public CheckoutController(ICheckoutService checkoutService, IUserAddressService userAddressService)
        {
            _checkoutService = checkoutService;
            _userAddressService = userAddressService;
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

            return View(summary);
        }

        [HttpPost]
        [ValidateAntiForgeryToken] 
        public async Task<IActionResult> PlaceOrder(Guid selectedAddressId)
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

            var result = await _checkoutService.TPlaceOrderAsync(userIdString, selectedAddressId);

            if (result)
            {
                TempData["SuccessMessage"] = "Your order has been successfully received!";
                return RedirectToAction("OrderSuccess");
            }

            TempData["ErrorMessage"] = "An error occurred while creating the order. Please check the stock and try again.";
            return RedirectToAction("Index");
        }

        [HttpGet]
        public IActionResult OrderSuccess()
        {
            return View();
        }
    }
}
