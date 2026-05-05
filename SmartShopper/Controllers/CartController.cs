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

                await _cartService.TAddToCartAsync(userId, createCartItemDto);

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

        [HttpPost]
        public async Task<IActionResult> UpdateCartItem([FromBody] UpdateCartItemDto updateCartItemDto)
        {
            var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userIdString == null)
                return Json(new { success = false, message = "Please sign in to update your cart." });

            try
            {
                await _cartService.TUpdateCartItemAsync(updateCartItemDto);

                return Json(new
                {
                    success = true,
                    message = "Cart updated successfully."
                });
            }
            catch (LogicException ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
            catch (Exception)
            {
                return Json(new { success = false, message = "An error occurred while updating the cart." });
            }
        }

        [HttpPost]
        public async Task<IActionResult> RemoveCartItem(Guid id)
        {
            if (id == Guid.Empty)
                return Json(new { success = false, message = "Invalid item identification." });

            try
            {
                await _cartService.TRemoveCartItemAsync(id);
                return Json(new { success = true, message = "Item removed from cart." });
            }
            catch (LogicException ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
            catch (Exception)
            {
                return Json(new { success = false, message = "An error occurred while removing the item." });
            }
        }

        [HttpPost]
        public async Task<IActionResult> ClearCart()
        {
            var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userIdString == null)
                return Json(new { success = false, message = "Please sign in." });

            try
            {
                var userId = Guid.Parse(userIdString);
                await _cartService.TClearCartAsync(userId);
                return Json(new { success = true, message = "Your cart has been cleared." });
            }
            catch (Exception)
            {
                return Json(new { success = false, message = "An error occurred while clearing the cart." });
            }
        }
    }
}
