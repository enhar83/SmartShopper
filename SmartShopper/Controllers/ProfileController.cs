using System.Security.Claims;
using Core_Layer.Dtos.ProfileDtos;
using Core_Layer.Exceptions;
using Core_Layer.IServices;
using Microsoft.AspNetCore.Mvc;

namespace SmartShopper.Controllers
{
    public class ProfileController : Controller
    {
        private readonly IProfileService _profileService;

        public ProfileController(IProfileService profileService)
        {
            _profileService = profileService;
        }

        public async Task<IActionResult> ViewProfile()
        {
            var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userIdString == null)
                return RedirectToAction("Login","Account");

            var userId = Guid.Parse(userIdString);

            var userProfile = await _profileService.TGetProfileAsync(userId);
            return View(userProfile);
        }

        [HttpGet]
        public async Task<IActionResult> GetProfileForEdit()
        {
            var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userIdString == null)
                return RedirectToAction("Login", "Account");

            if (string.IsNullOrEmpty(userIdString))
                return Unauthorized();

            var userId = Guid.Parse(userIdString);

            var profile = await _profileService.TGetProfileAsync(userId);

            if (profile == null)
                return NotFound(new { message = "User not found" });

            return Json(new
            {
                id = profile.Id,
                name = profile.Name,
                surname = profile.Surname,
                userName = profile.UserName,
                phoneNumber = profile.PhoneNumber,
                userImage = profile.ImageUrl
            });
        }

        [HttpPost]
        [ValidateAntiForgeryToken] 
        public async Task<IActionResult> EditProfile(EditProfileDto editProfileDto)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage);
                return BadRequest(new { succeeded = false, errors = errors });
            }

            try
            {
                var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (editProfileDto.Id.ToString() != currentUserId)
                {
                    return Forbid(); 
                }

                var result = await _profileService.TEditProfileAsync(editProfileDto);

                if (result.Succeeded)
                {
                    return Ok(new { succeeded = true, message = "Profile updated successfully!" });
                }

                var identityErrors = result.Errors.Select(e => e.Description);
                return BadRequest(new { succeeded = false, errors = identityErrors });
            }
            catch (LogicException ex)
            {
                return BadRequest(new { succeeded = false, errors = new[] { ex.Message } });
            }
            catch (Exception)
            {
                return StatusCode(500, new { succeeded = false, errors = new[] { "An unexpected error occurred on the server." } });
            }
        }
    }
}
