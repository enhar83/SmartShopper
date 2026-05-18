using Core_Layer.IServices;
using Microsoft.AspNetCore.Mvc;

namespace SmartShopper.ViewComponents.Comment
{
    public class _CommentListForProductDetailComponentPartial:ViewComponent
    {
        private readonly ICommentService _commentService;

        public _CommentListForProductDetailComponentPartial(ICommentService commentService)
        {
            _commentService = commentService;
        }

        public async Task<IViewComponentResult> InvokeAsync(Guid productId)
        {
            var comments = await _commentService.TGetApprovedCommentsByProductIdAsync(productId);
            return View(comments);
        }
    }
}
