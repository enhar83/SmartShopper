using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;
using Core_Layer.Dtos.ProfileDtos;
using Core_Layer.Exceptions;
using Core_Layer.IServices;
using Entity_Layer;
using Microsoft.AspNetCore.Identity;

namespace Business_Layer.Managers
{
    public class ProfileManager:IProfileService
    {
        private readonly UserManager<AppUser> _userManager;
        private readonly IMapper _mapper;

        public ProfileManager(UserManager<AppUser> userManager, IMapper mapper)
        {
            _userManager = userManager;
            _mapper = mapper;
        }

        public async Task<ViewProfileDto> TGetProfileAsync(Guid id)
        {
            if (id == Guid.Empty)
                throw new LogicException("Id", "Invalid User Id");

            var userProfile = await _userManager.FindByIdAsync(id.ToString());
            if (userProfile == null)
                throw new LogicException("Id", "The user not found.");

            return _mapper.Map<ViewProfileDto>(userProfile);
        }
    }
}
