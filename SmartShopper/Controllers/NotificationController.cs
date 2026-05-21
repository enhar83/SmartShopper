using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Core_Layer.IServices;
using System;
using System.Security.Claims;
using System.Threading.Tasks;

namespace WebUI.Controllers 
{
    public class NotificationController : Controller
    {
        private readonly INotificationService _notificationService;

        public NotificationController(INotificationService notificationService)
        {
            _notificationService = notificationService;
        }

        public async Task<IActionResult> Index()
        {
            var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userIdString))
                return RedirectToAction("Login", "Account");

            Guid userId = Guid.Parse(userIdString);

            var notifications = await _notificationService.TGetNotificationHistoryAsync(userId);

            return View(notifications);
        }

        [HttpPost]
        public async Task<IActionResult> MarkAsRead(Guid id)
        {
            try
            {
                await _notificationService.TMarkAsReadAsync(id);
                return Json(new { success = true });
            }
            catch
            {
                return Json(new { success = false });
            }
        }

        [HttpPost]
        public async Task<IActionResult> MarkAllAsRead()
        {
            try
            {
                var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (string.IsNullOrEmpty(userIdString))
                    return Json(new { success = false, message = "User not found." });

                Guid userId = Guid.Parse(userIdString);
                await _notificationService.TMarkAllAsReadAsync(userId);

                return Json(new { success = true });
            }
            catch
            {
                return Json(new { success = false });
            }
        }
    }
}