using System.Security.Claims;
using Core_Layer.Dtos.CommentDtos;
using Core_Layer.Exceptions;
using Core_Layer.IServices;
using Microsoft.AspNetCore.Mvc;

namespace SmartShopper.Controllers
{
    public class CommentController : Controller
    {
        private readonly ICommentService _commentService;

        public CommentController(ICommentService commentService)
        {
            _commentService = commentService;
        }

        [HttpPost]
        [ValidateAntiForgeryToken] 
        public async Task<IActionResult> AddComment(CreateCommentDto createCommentDto)
        {
            if (!ModelState.IsValid)
            {
                TempData["ErrorMessage"] = "Please correct any errors in the form.";
                return RedirectToAction("Details", "Product", new { id = createCommentDto.ProductId });
            }

            var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (string.IsNullOrEmpty(userIdString) || !Guid.TryParse(userIdString, out Guid userId))
                return RedirectToAction("Login", "Account");

            try
            {
                await _commentService.TAddAsync(createCommentDto, userId);
                TempData["SuccessMessage"] = "Your comment has been successfully received and forwarded for administrator approval.";
            }
            catch (LogicException ex)
            {
                TempData["WarningMessage"] = ex.Message;
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "System Error: " + ex.Message + " | Detail: " + ex.InnerException?.Message;
            }

            return RedirectToAction("ProductDetails", "Product", new { id = createCommentDto.ProductId });
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (string.IsNullOrEmpty(userIdString) || !Guid.TryParse(userIdString, out Guid userId))
                return RedirectToAction("Login", "Auth");

            var comments = await _commentService.TGetCommentsByUserIdAsync(userId);

            return View(comments);
        }
    }
}
