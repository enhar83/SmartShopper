using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;
using Core_Layer.Dtos.UserAddressDtos;
using Core_Layer.Exceptions;
using Core_Layer.IRepositories;
using Core_Layer.IServices;
using Entity_Layer;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;

namespace Business_Layer.Managers
{
    public class UserAddressManager : IUserAddressService
    {
        private readonly IUserAddressRepository _userAddressRepository;
        private readonly IUnitOfWork _uow;
        private readonly IMapper _mapper;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public UserAddressManager(IUserAddressRepository userAddressRepository, IUnitOfWork uow, IMapper mapper, IHttpContextAccessor httpContextAccessor)
        {
            _userAddressRepository = userAddressRepository;
            _uow = uow;
            _mapper = mapper;
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task TAddUserAddressAsync(AddUserAddressDto addUserAddressDto)
        {
            var userAddress = _mapper.Map<UserAddress>(addUserAddressDto);

            var userIdClaim = _httpContextAccessor.HttpContext?.User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (string.IsNullOrEmpty(userIdClaim))
                throw new LogicException("Auth", "User authentication required.");

            userAddress.AppUserId = Guid.Parse(userIdClaim); 
            userAddress.CreatedDate = DateTime.Now;          
            userAddress.IsDeleted = false;                  

            await _userAddressRepository.AddAsync(userAddress);
            await _uow.SaveAsync();
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
