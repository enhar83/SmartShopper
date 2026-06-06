using System.Security.Claims;
using Core_Layer.Dtos.NotificationDtos;
using Core_Layer.IServices;
using Microsoft.AspNetCore.Mvc;

namespace SmartShopper.ViewComponents.Notification
{
    public class _NavbarNotificationListComponentPartial : ViewComponent
    {
        private readonly INotificationService _notificationService;
        public _NavbarNotificationListComponentPartial(INotificationService notificationService)
        {
            _notificationService = notificationService;
        }

        public async Task<IViewComponentResult> InvokeAsync()
        {
            var userIdString = HttpContext.User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (string.IsNullOrEmpty(userIdString))
                return View(new List<NotificationListDto>()); 

            Guid userId = Guid.Parse(userIdString);

            ViewBag.UnreadCount = await _notificationService.TGetUnreadNotificationCountAsync(userId);
            var notifications = await _notificationService.TGetUserNotificationsAsync(userId);

            return View("Default", notifications);
        }
    }
}
