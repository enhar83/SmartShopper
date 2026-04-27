using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Core_Layer.Dtos.AuthDtos;
using Microsoft.AspNetCore.Identity;

namespace Core_Layer.IServices
{
    public interface IAuthService
    {
        Task<IdentityResult> TRegisterAsync(RegisterDto registerDto);
        Task<bool> TConfirmEmailAsync(ConfirmUserEmailDto confirmUserEmailDto);
        Task<List<UserListDto>> TGetUserListAsync();
    }
}
