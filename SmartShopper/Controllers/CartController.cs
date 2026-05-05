using System.Security.Claims;
using System.Threading.Tasks;
using Core_Layer.Dtos.CartDtos;
using Core_Layer.Exceptions;
using Core_Layer.IServices;
using Microsoft.AspNetCore.Mvc;

namespace SmartShopper.Controllers
{
    public class CartController : Controller
    {
        private readonly ICartService _cartService;

        public CartController(ICartService cartService)
        {
            _cartService = cartService;
        }

        public IActionResult Index()
        {
            return View();
        }

        public async Task<IActionResult> GetCartData()
        {
            var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userIdString == null) 
                return RedirectToAction("Login", "Account");

            var userId = Guid.Parse(userIdString);
            var cartDto = await _cartService.TGetUserCartAsync(userId);

            return Json(cartDto);
        }

        [HttpPost]
        public async Task<IActionResult> AddToCart([FromBody] CreateCartItemDto createCartItemDto)
        {
            var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userIdString == null)
                return Json(new { success = false, message = "Please sign in." });

            try
            {
                var userId = Guid.Parse(userIdString);

                await _cartService.AddToCartAsync(userId, createCartItemDto);

                return Json(new { success = true, message = "The product has been successfully added to the cart." });
            }
            catch (LogicException ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }
    }
}
