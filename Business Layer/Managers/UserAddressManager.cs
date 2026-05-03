using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;
using Core_Layer.Dtos.UserAddressDtos;
using Core_Layer.IRepositories;
using Core_Layer.IServices;
using Entity_Layer;
using Microsoft.EntityFrameworkCore;

namespace Business_Layer.Managers
{
    public class UserAddressManager : IUserAddressService
    {
        private readonly IUserAddressRepository _userAddressRepository;
        private readonly IUnitOfWork _uow;
        private readonly IMapper _mapper;

        public UserAddressManager(IUserAddressRepository userAddressRepository, IUnitOfWork uow, IMapper mapper)
        {
            _userAddressRepository = userAddressRepository;
            _uow = uow;
            _mapper = mapper;
        }

        public async Task<List<UserAddressListDto>> TGetUserAddressListAsync(Guid userId)
        {
            if (userId == Guid.Empty)
                return new List<UserAddressListDto>();

            var addresses = await _userAddressRepository
                .Where(x => x.AppUserId == userId)
                .OrderByDescending(x => x.CreatedDate) 
                .ToListAsync();

            return _mapper.Map<List<UserAddressListDto>>(addresses);
        }

    }
}
