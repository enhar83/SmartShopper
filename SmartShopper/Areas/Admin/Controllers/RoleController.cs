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

        [HttpGet]
        public async Task<IActionResult> GetRole(Guid id)
        {
            var roles = await _roleService.TGetAllRolesAsync();
            var role = roles.FirstOrDefault(x => x.Id == id);

            if (role == null) return NotFound();

            return Json(role);
        }

        [HttpPost]
        public async Task<IActionResult> UpdateRole(UpdateRoleDto updateRoleDto)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage);
                return Json(new { success = false, message = string.Join("<br>", errors) });
            }

            try
            {
                var result = await _roleService.TUpdateRoleAsync(updateRoleDto);

                if (result.Succeeded)
                {
                    return Json(new { success = true, message = "Role updated successfully!" });
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
                return Json(new { success = false, message = "An unexpected error occurred during update." });
            }
        }

        [HttpPost]
        public async Task<IActionResult> DeleteRole(DeleteRoleDto deleteRoleDto)
        {
            if (deleteRoleDto.Id == Guid.Empty)
            {
                return Json(new { success = false, message = "Invalid role ID." });
            }

            try
            {
                var result = await _roleService.TDeleteRoleAsync(deleteRoleDto);

                if (result.Succeeded)
                {
                    return Json(new { success = true, message = "Role has been deleted successfully." });
                }

                var errorMessages = string.Join("<br>", result.Errors.Select(e => e.Description));
                return Json(new { success = false, message = errorMessages });
            }
            catch (LogicException ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
            catch (Exception)
            {
                return Json(new { success = false, message = "An unexpected error occurred during the deletion process." });
            }
        }

        public async Task<IActionResult> GetUsersInRole(string roleName)
        {
            if (string.IsNullOrEmpty(roleName))
                return BadRequest("Role name is required.");

            var users = await _roleService.TUsersInRoleAsync(roleName);

            return Json(users);
        }

        [HttpPost]
        public async Task<IActionResult> RemoveUserFromRole(RemoveUserFromRoleDto removeUserFromRoleDto)
        {
            try
            {
                var result = await _roleService.TRemoveUserFromRoleAsync(removeUserFromRoleDto);

                if (result.Succeeded)
                {
                    return Json(new
                    {
                        success = true,
                        message = "The user was successfully removed from the role."
                    });
                }

                var errorMessages = string.Join("<br>", result.Errors.Select(e => e.Description));
                return Json(new { success = false, message = errorMessages });
            }
            catch (LogicException ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
            catch (Exception)
            {
                return Json(new { success = false, message = "An unexpected error occurred during the process." });
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetAssignRole(Guid id)
        {
            var values = await _roleService.TGetRolesForUserAsync(id);
            return Json(values);
        }

        [HttpPost]
        public async Task<IActionResult> AssignRole(Guid userId, List<AssignRoleDto> assignRoleDto)
        {
            try
            {
                if (userId == Guid.Empty)
                    return Json(new { success = false, message = "Invalid User ID." });

                await _roleService.TAssignRoleAsync(userId, assignRoleDto);
                return Json(new { success = true, message = "Roles updated successfully." });
            }
            catch (LogicException ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
            catch (Exception)
            {
                return Json(new { success = false, message = "An error occurred while assigning roles." });
            }
        }
    }
}
