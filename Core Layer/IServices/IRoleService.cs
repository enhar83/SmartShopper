using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Core_Layer.Dtos.RoleDtos;
using Microsoft.AspNetCore.Identity;

namespace Core_Layer.IServices
{
    public interface IRoleService
    {
        Task<IdentityResult> TCreateRoleAsync(CreateRoleDto createRoleDto);
        Task<List<RoleListDto>> TGetAllRolesAsync();
        Task<IdentityResult> TUpdateRoleAsync(UpdateRoleDto updateRoleDto);
        Task<IdentityResult> TDeleteRoleAsync(DeleteRoleDto deleteRoleDto);
        Task<List<UsersInRoleDto>> TUsersInRoleAsync(string roleName);
    }
}
