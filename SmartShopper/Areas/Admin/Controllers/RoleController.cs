using Core_Layer.Dtos.RoleDtos;
using Core_Layer.Exceptions;
using Core_Layer.IServices;
using Microsoft.AspNetCore.Mvc;

namespace SmartShopper.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class RoleController : Controller
    {
        private readonly IRoleService _roleService;
        public RoleController(IRoleService roleService)
        {
            _roleService = roleService;
        }

        public async Task<IActionResult> RoleList()
        {
            var roles = await _roleService.TGetAllRolesAsync();
            return View(roles);
        }

        [HttpPost]
        public async Task<IActionResult> CreateRole(CreateRoleDto createRoleDto)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage);
                return Json(new { success = false, message = string.Join("<br>", errors) });
            }

            try
            {
                var result = await _roleService.TCreateRoleAsync(createRoleDto);

                if (result.Succeeded)
                {
                    return Json(new { success = true, message = "Role created successfully!" });
                }

                var identityErrors = string.Join("<br>", result.Errors.Select(e => e.Description));
                return Json(new { success = false, message = identityErrors });
            }
            catch (LogicException ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
            catch (Exception)
            {
                return Json(new { success = false, message = "An unexpected error occurred." });
            }
        }
    }
}
