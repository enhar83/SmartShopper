using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;
using Core_Layer.Dtos.RoleDtos;
using Core_Layer.Exceptions;
using Core_Layer.IServices;
using Entity_Layer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Business_Layer.Managers
{
    public class RoleManager : IRoleService
    {
        private readonly RoleManager<AppRole> _roleManager;
        private readonly UserManager<AppUser> _userManager;
        private readonly IMapper _mapper;

        public RoleManager(RoleManager<AppRole> roleManager, UserManager<AppUser> userManager, IMapper mapper)
        {
            _roleManager = roleManager;
            _userManager = userManager;
            _mapper = mapper;
        }

        public async Task TAssignRoleAsync(Guid userId, List<AssignRoleDto> assignRoleDto)
        {
            var user = await _userManager.FindByIdAsync(userId.ToString());
            if (user == null)
                throw new LogicException("User", "User not found.");

            foreach (var item in assignRoleDto)
            {
                bool isInRole = await _userManager.IsInRoleAsync(user, item.RoleName);

                if (item.RoleExist && !isInRole)
                    await _userManager.AddToRoleAsync(user, item.RoleName);

                else if (!item.RoleExist && isInRole)
                    await _userManager.RemoveFromRoleAsync(user, item.RoleName);
            }
        }

        public async Task<List<AssignRoleDto>> TGetRolesForUserAsync(Guid userId)
        {
            var user = await _userManager.FindByIdAsync(userId.ToString());
            if (user == null)
                throw new LogicException("User", "User not found.");

            var allRoles = _roleManager.Roles.ToList();
            var userRoles = await _userManager.GetRolesAsync(user);

            var assignRoleList = _mapper.Map<List<AssignRoleDto>>(allRoles, opt =>
            {
                opt.AfterMap((src, dest) =>
                {
                    foreach (var item in dest)
                    {
                        item.RoleExist = userRoles.Contains(item.RoleName);
                    }
                });
            });

            return assignRoleList;
        }

        public async Task<IdentityResult> TCreateRoleAsync(CreateRoleDto createRoleDto)
        {
            var roleExist = await _roleManager.RoleExistsAsync(createRoleDto.RoleName);

            if (roleExist)
                throw new LogicException("RoleName", "This role already exists in the system.");

            var role = _mapper.Map<AppRole>(createRoleDto);
            role.CreatedDate = DateTime.Now;
            role.IsDeleted = false;

            var result = await _roleManager.CreateAsync(role);
            return result;
        }

        public async Task<IdentityResult> TDeleteRoleAsync(DeleteRoleDto deleteRoleDto)
        {
            var role = await _roleManager.FindByIdAsync(deleteRoleDto.Id.ToString());
            if (role == null)
                throw new LogicException("RoleName", "The role not found");

            if (role.Name == "Admin" )
                throw new LogicException("RoleName", "System protected role cannot be deleted!");

            var result = await _roleManager.DeleteAsync(role);
            return result;
        }

        public async Task<List<RoleListDto>> TGetAllRolesAsync()
        {
            var roles = await _roleManager.Roles.AsNoTracking().ToListAsync();

            var rolesList = _mapper.Map<List<RoleListDto>>(roles);
            return rolesList;
        }

        public async Task<IdentityResult> TRemoveUserFromRoleAsync(RemoveUserFromRoleDto removeUserFromRoleDto)
        {
            var user = await _userManager.FindByIdAsync(removeUserFromRoleDto.Id.ToString());

            if (user == null)
                throw new LogicException("User", "Kullanıcı sistemde bulunamadı.");

            var result = await _userManager.RemoveFromRoleAsync(user, removeUserFromRoleDto.RoleName);
            return result;
        }

        public async Task<IdentityResult> TUpdateRoleAsync(UpdateRoleDto updateRoleDto)
        {
            var role = await _roleManager.FindByIdAsync(updateRoleDto.Id.ToString());
            if (role == null)
                throw new LogicException("Role", "The role was not found.");

            if (role.Name != updateRoleDto.RoleName)
            {
                var isRoleExists = await _roleManager.RoleExistsAsync(updateRoleDto.RoleName);
                if (isRoleExists)
                    throw new LogicException("RoleName", "This role name already exists in the system.");
            }

            _mapper.Map(updateRoleDto,role);

            var result = await _roleManager.UpdateAsync(role);
            return result;
        }

        public async Task<List<UsersInRoleDto>> TUsersInRoleAsync(string roleName)
        {
            if (string.IsNullOrEmpty(roleName))
                return new List<UsersInRoleDto>();

            var users = await _userManager.GetUsersInRoleAsync(roleName);

            if (users == null)
                return new List<UsersInRoleDto>();

            return _mapper.Map<List<UsersInRoleDto>>(users.ToList());
        }
    }
}
