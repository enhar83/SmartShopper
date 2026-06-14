using System.Security.Claims;
using Core_Layer.IServices;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace SmartShopper.Controllers
{
    [Authorize(Roles = "Admin, Member")]
    public class RecommendationController : Controller
    {
        private readonly IRecommendationService _recommendationService;

        public RecommendationController(IRecommendationService recommendationService)
        {
            _recommendationService = recommendationService;
        }

        public async Task<IActionResult> GetOrderRecommendations()
        {
            var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            var data = await _recommendationService.TGetOrderBasedRecommendationsAsync(userId, 12);
            return View(data);
        }

        public async Task<IActionResult> GetFavoriteRecommendations()
        {
            var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            var data = await _recommendationService.TGetFavoriteBasedRecommendationsAsync(userId, 12);
            return View(data);
        }

        public async Task<IActionResult> GetCartRecommendations()
        {
            var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            var data = await _recommendationService.TGetCartBasedRecommendationsAsync(userId, 12);
            return View(data);
        }
    }
}
