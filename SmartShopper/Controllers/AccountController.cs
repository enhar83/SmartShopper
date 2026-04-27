using Core_Layer.Dtos.AuthDtos;
using Core_Layer.Exceptions;
using Core_Layer.IServices;
using Microsoft.AspNetCore.Mvc;

namespace SmartShopper.Controllers
{
    public class AccountController : Controller
    {
        private readonly IAuthService _authService;

        public AccountController(IAuthService authService)
        {
            _authService = authService;
        }

        [HttpGet]
        public IActionResult Register()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Register(RegisterDto registerDto)
        {
            if (!ModelState.IsValid)
                return View(registerDto);

            try
            {
                var result = await _authService.TRegisterAsync(registerDto);

                if (result.Succeeded)
                {
                    TempData["UserEmail"] = registerDto.Email;
                    TempData["SuccessRegisterMessage"] = "Register completed successfully!";
                    return RedirectToAction("VerifyEmail");
                }

                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError("", error.Description);
                }
            }
            catch (LogicException ex)
            {
                ModelState.AddModelError(ex.PropertyName ?? "", ex.Message);
            }
            catch (Exception)
            {
                ModelState.AddModelError("", "An unexpected error occurred. Please try again later.");
            }

            return View(registerDto);
        }

        [HttpGet]
        public IActionResult VerifyEmail()
        {
            var email = TempData["UserEmail"]?.ToString();
            if (string.IsNullOrEmpty(email))
                return RedirectToAction("Register");

            ViewBag.Email = email;
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> VerifyEmail(ConfirmUserEmailDto confirmUserEmailDto)
        {
            try
            {
                var result = await _authService.TConfirmEmailAsync(confirmUserEmailDto);
                if (result)
                {
                    TempData["SuccessEmailConfirmedMessage"] = "Email verified successfully!";
                    return RedirectToAction("Login");
                }
            }
            catch (LogicException ex)
            {
                ModelState.AddModelError("", ex.Message);
            }

            ViewBag.Email = confirmUserEmailDto.Email;
            return View(confirmUserEmailDto);
        }
    }
}
