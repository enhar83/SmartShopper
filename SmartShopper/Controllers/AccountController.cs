using Core_Layer.Dtos.AuthDtos;
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

            var result = await _authService.RegisterAsync(registerDto);
            if (result.Succeeded)
            {
                TempData["SuccessRegisterMessage"] = "Registration successful. You can now log in.";
                return RedirectToAction("Login");
            }
                
            foreach (var error in result.Errors)
                ModelState.AddModelError("", error.Description);

            return View(registerDto);
        }
    }
}
