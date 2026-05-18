using Core_Layer.Exceptions;
using Core_Layer.IServices;
using Microsoft.AspNetCore.Mvc;

namespace SmartShopper.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class CommentController : Controller
    {
        private readonly ICommentService _commentService;

        public CommentController(ICommentService commentService)
        {
            _commentService = commentService;
        }

        public async Task<IActionResult> CommentList()
        {
            var comments = await _commentService.TGetAllCommentsForAdminAsync();
            return View(comments);
        }

        [HttpPost]
        public async Task<IActionResult> ToggleApproval(Guid id)
        {
            try
            {
                await _commentService.TToggleCommentApprovalAsync(id);

                return Json(new { success = true, message = "Comment visibility status has been successfully updated." });
            }
            catch (LogicException ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
            catch (Exception)
            {
                return Json(new { success = false, message = "A technical error occurred while updating the comment status." });
            }
        }
    }
}
