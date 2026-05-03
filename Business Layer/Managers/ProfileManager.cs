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
using Microsoft.EntityFrameworkCore;
using MimeKit.Encodings;

namespace Business_Layer.Managers
{
    public class ProfileManager : IProfileService
    {
        private readonly UserManager<AppUser> _userManager;
        private readonly IMapper _mapper;

        public ProfileManager(UserManager<AppUser> userManager, IMapper mapper)
        {
            _userManager = userManager;
            _mapper = mapper;
        }

        public async Task<IdentityResult> TEditProfileAsync(EditProfileDto editProfileDto)
        {
            var user = await _userManager.FindByIdAsync(editProfileDto.Id.ToString());
            if (user == null)
                throw new LogicException("Id", "User record could not be found.");

            if (user.UserName != editProfileDto.UserName && await _userManager.Users.AnyAsync(u => u.UserName == editProfileDto.UserName))
                throw new LogicException("UserName", "This username is already taken.");

            if (user.PhoneNumber != editProfileDto.PhoneNumber && await _userManager.Users.AnyAsync(u => u.PhoneNumber == editProfileDto.PhoneNumber))
                throw new LogicException("PhoneNumber", "This phone number is already registered.");

            if (editProfileDto.UserImage != null && editProfileDto.UserImage.Length > 0)
            {
                var rootPath = Directory.GetCurrentDirectory();
                var extension = Path.GetExtension(editProfileDto.UserImage.FileName);
                var imageName = $"{Guid.NewGuid()}{extension}";

                var relativePath = $"images/users/{imageName}";
                var absolutePath = Path.Combine(rootPath, "wwwroot", "images", "users", imageName);

                var directoryPath = Path.GetDirectoryName(absolutePath);
                if (!Directory.Exists(directoryPath))
                    Directory.CreateDirectory(directoryPath!);

                if (!string.IsNullOrEmpty(user.ImageUrl))
                {
                    var oldFilePath = Path.Combine(rootPath, "wwwroot", user.ImageUrl.TrimStart('/'));
                    if (File.Exists(oldFilePath))
                        File.Delete(oldFilePath);
                }

                using (var stream = new FileStream(absolutePath, FileMode.Create))
                {
                    await editProfileDto.UserImage.CopyToAsync(stream);
                }

                user.ImageUrl = "/" + relativePath;
            }

            _mapper.Map(editProfileDto, user);

            var result = await _userManager.UpdateAsync(user);
            if (result.Succeeded)
                await _userManager.UpdateSecurityStampAsync(user);

            return result;
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
