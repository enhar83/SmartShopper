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

        public async Task<List<RoleListDto>> TGetAllRolesAsync()
        {
            var roles = await _roleManager.Roles.AsNoTracking().ToListAsync();

            var rolesList = _mapper.Map<List<RoleListDto>>(roles);
            return rolesList;
        }
    }
}
