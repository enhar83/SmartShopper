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

            var result = await _roleManager.CreateAsync(role);
            return result;
        }
    }
}
