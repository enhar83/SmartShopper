using System.Security.Claims;
using System.Threading.Tasks;
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
    }
}
