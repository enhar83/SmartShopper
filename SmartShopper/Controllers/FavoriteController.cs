using Core_Layer.Exceptions;
using Core_Layer.IServices;
using Microsoft.AspNetCore.Mvc;

namespace SmartShopper.Controllers
{
    public class FavoriteController : Controller
    {
        private readonly IFavoriteService _favoriteService;

        public FavoriteController(IFavoriteService favoriteService)
        {
            _favoriteService = favoriteService;
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ToggleFavorite(Guid productId)
        {
            if (productId == Guid.Empty)
            {
                return BadRequest(new { succeeded = false, message = "Invalid product identification." });
            }

            try
            {
                bool isAdded = await _favoriteService.TToggleFavoriteAsync(productId);

                return Ok(new
                {
                    succeeded = true,
                    isFavorite = isAdded,
                    message = isAdded ? "Product added to wishlist." : "Product removed from wishlist."
                });
            }
            catch (LogicException ex)
            {
                return BadRequest(new { succeeded = false, errors = new[] { ex.Message } });
            }
            catch (Exception)
            {
                return StatusCode(500, new { succeeded = false, errors = new[] { "A technical error occurred during favorite operation." } });
            }
        }
    }
}
