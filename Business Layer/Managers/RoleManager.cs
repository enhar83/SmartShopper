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
    public class RoleManager: IRoleService
    {
        private readonly RoleManager<AppRole> _roleManager;
        private readonly IMapper _mapper;

        public RoleManager(RoleManager<AppRole> roleManager, IMapper mapper)
        {
            _roleManager = roleManager;
            _mapper = mapper;
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
    }
}
