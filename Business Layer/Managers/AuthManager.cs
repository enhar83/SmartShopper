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
using Microsoft.EntityFrameworkCore;
using MimeKit.Encodings;

namespace Business_Layer.Managers
{
    public class AuthManager : IAuthService
    {
        private readonly UserManager<AppUser> _userManager;
        private readonly SignInManager<AppUser> _signInManager;
        private readonly IEmailActivationService _emailActivationService;
        private readonly IMapper _mapper;

        public AuthManager(UserManager<AppUser> userManager, SignInManager<AppUser> signInManager, IEmailActivationService emailActivationService, IMapper mapper)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _emailActivationService = emailActivationService;
            _mapper = mapper;
        }

        public async Task<bool> TConfirmEmailAsync(ConfirmUserEmailDto confirmUserEmailDto)
        {
            var user = await _userManager.FindByEmailAsync(confirmUserEmailDto.Email);
            if (user == null)
                throw new LogicException("Email", "Email could not found.");

            if (user.ActivationCode != confirmUserEmailDto.ActivationCode)
                throw new LogicException("Code", "The verification code is incorrect.");

            user.EmailConfirmed = true;
            user.ActivationCode = null; //doğrulama yapıldıktan sonra dbde kaldırılıyor. 

            var result = await _userManager.UpdateAsync(user);

            if (!result.Succeeded)
                throw new Exception("An error occurred while confirming the email.");

            return true;
        }

        public async Task<List<UserListDto>> TGetUserListAsync()
        {
            var users = await _userManager.Users
                .OrderByDescending(x => x.CreatedDate)
                .ToListAsync();

            var userListDto = _mapper.Map<List<UserListDto>>(users);

            return userListDto;
        }

        public async Task<bool> TLoginAsync(LoginDto loginDto)
        {
            var user = await _userManager.FindByEmailAsync(loginDto.UsernameOrEmail)
                       ?? await _userManager.FindByNameAsync(loginDto.UsernameOrEmail);

            if (user == null)
                throw new LogicException("UsernameOrEmail", "Invalid username/email or password");

            if (!user.EmailConfirmed)
                throw new LogicException("EmailNotConfirmed", "Please verify your email.");

            var result = await _signInManager.PasswordSignInAsync(user, loginDto.Password, isPersistent: false, lockoutOnFailure: true);
            return result.Succeeded;
        }

        public async Task<IdentityResult> TRegisterAsync(RegisterDto registerDto)
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

            if (result.Succeeded)
            {
                await _userManager.AddToRoleAsync(appUser, "Member");
                string code = await CreateEmailTokenAsync(appUser); //appuser nesnesi üzerinde işlem yapılacağı için direkt yollanması best practicedir. 
                await _emailActivationService.TSendConfirmEmailAsync(appUser.Email!, code); //gelen kodu ve maili emailActivationService içerisindeki metota yollanır. 
            }

            return result;
        }

        public async Task TResendVerificationCodeAsync(string email)
        {
            var user = await _userManager.FindByEmailAsync(email);

            if (user == null)
                throw new LogicException("Email", "User not found.");

            if (user.EmailConfirmed)
                throw new LogicException("Email", "This account is already verified.");

            string newCode = await CreateEmailTokenAsync(user);

            await _emailActivationService.TSendConfirmEmailAsync(user.Email!, newCode);
        }

        private async Task<string> CreateEmailTokenAsync(AppUser user)
        {
            var code = new Random().Next(100000, 999999).ToString();

            user.ActivationCode = code;

            var result = await _userManager.UpdateAsync(user);

            if (!result.Succeeded)
                throw new Exception("An error occurred while generating the verification code.");

            return code;
        }
    }
}
