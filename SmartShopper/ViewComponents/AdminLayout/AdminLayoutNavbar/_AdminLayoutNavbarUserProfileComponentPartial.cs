using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using SmartShopper.Areas.Admin.Models;

namespace SmartShopper.ViewComponents.AdminLayout.AdminLayoutNavbar
{
    public class _AdminLayoutNavbarUserProfileComponentPartial:ViewComponent
    {
        public IViewComponentResult Invoke()
        {
            var name = UserClaimsPrincipal.FindFirstValue("name");
            var surname = UserClaimsPrincipal.FindFirstValue("surname");
            var image = UserClaimsPrincipal.FindFirstValue("imageurl");

            var model = new AdminNavbarUserProfileViewModel
            {
                FullName = $"{name} {surname}".Trim(),
                ImageUrl = string.IsNullOrEmpty(image) || image.Contains("default") ? null : image,
                RoleName = UserClaimsPrincipal.FindFirstValue(ClaimTypes.Role) ?? "Admin",
                Initials = $"{(name?.Length > 0 ? name[0] : ' ')}{(surname?.Length > 0 ? surname[0] : ' ')}".Trim().ToUpper()
            };

            return View(model);
        }
    }
}
