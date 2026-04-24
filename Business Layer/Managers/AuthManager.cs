using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;
using Core_Layer.Dtos.AuthDtos;
using Core_Layer.Exceptions;
using Core_Layer.IServices;
using Entity_Layer;
using Microsoft.AspNetCore.Identity;

namespace Business_Layer.Managers
{
    public class AuthManager : IAuthService
    {
        private readonly UserManager<AppUser> _userManager;
        private readonly IMapper _mapper;

        public AuthManager(UserManager<AppUser> userManager, IMapper mapper)
        {
            _userManager = userManager;
            _mapper = mapper;
        }

        public async Task<IdentityResult> RegisterAsync(RegisterDto registerDto)
        {
            var existingUserName = await _userManager.FindByNameAsync(registerDto.UserName);
            if (existingUserName != null)
                throw new LogicException("UserName", "This username is already in use.");

            var existingEmail = await _userManager.FindByEmailAsync(registerDto.Email);
            if (existingEmail != null)
                throw new LogicException("Email", "This email is already in use.");

            var appUser = _mapper.Map<AppUser>(registerDto);
            appUser.CreatedDate = DateTime.Now;
            appUser.IsDeleted = false;

            var result = await _userManager.CreateAsync(appUser, registerDto.Password);

            return result;
        }
    }
}
