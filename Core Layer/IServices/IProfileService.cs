using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Core_Layer.Dtos.ProfileDtos;
using Microsoft.AspNetCore.Identity;

namespace Core_Layer.IServices
{
    public interface IProfileService
    {
        Task<ViewProfileDto> TGetProfileAsync(Guid id);
        Task<IdentityResult> TEditProfileAsync(EditProfileDto editProfileDto);
    }
}
