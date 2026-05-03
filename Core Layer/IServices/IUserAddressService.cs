using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Core_Layer.Dtos.UserAddressDtos;

namespace Core_Layer.IServices
{
    public interface IUserAddressService
    {
        Task<List<UserAddressListDto>> TGetUserAddressListAsync(Guid userId);
        Task TAddUserAddressAsync(AddUserAddressDto addUserAddressDto);
        Task TUpdateUserAddressAsync(UpdateUserAddressDto updateUserAddressDto);
    }
}
