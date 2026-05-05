using System.Security.Claims;
using Core_Layer.Dtos.CartDtos;
using Core_Layer.IServices;
using Microsoft.AspNetCore.Mvc;

namespace SmartShopper.ViewComponents.Cart
{
    public class _CartNavbarComponentPartial:ViewComponent
    {
        private readonly ICartService _cartService;
        public _CartNavbarComponentPartial(ICartService cartService)
        {
            _cartService = cartService;
        }

        public async Task<IViewComponentResult> InvokeAsync()
        {
            var userIdClaim = ((ClaimsPrincipal)User).FindFirstValue(ClaimTypes.NameIdentifier);

            if (string.IsNullOrEmpty(userIdClaim))
                return View(new CartDto { Items = new List<CartItemDto>() });

            var userId = Guid.Parse(userIdClaim);
            var cart = await _cartService.TGetUserCartAsync(userId);

            return View(cart);
        }
    }
}
