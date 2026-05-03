using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;
using Bogus.DataSets;
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
            var userIdClaim = _httpContextAccessor.HttpContext?.User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userIdClaim))
                throw new LogicException("Auth", "User authentication required.");

            var currentUserId = Guid.Parse(userIdClaim);

            bool isTitleExists = await _userAddressRepository.AnyAsync(x =>
                x.AppUserId == currentUserId &&
                x.Title.ToLower() == addUserAddressDto.Title.ToLower());

            if (isTitleExists)
                throw new LogicException("Duplicate", $"You already have an address titled '{addUserAddressDto.Title}'.");

            bool isAddressExists = await _userAddressRepository.AnyAsync(x =>
                x.AppUserId == currentUserId &&
                x.FullAddress.ToLower() == addUserAddressDto.FullAddress.ToLower());

            if (isAddressExists)
                throw new LogicException("Duplicate", "This full address is already registered in your account.");

            var userAddress = _mapper.Map<UserAddress>(addUserAddressDto);
            userAddress.AppUserId = currentUserId;
            userAddress.CreatedDate = DateTime.Now;
            userAddress.IsDeleted = false;

            await _userAddressRepository.AddAsync(userAddress);
            await _uow.SaveAsync();
        }

        public async Task TDeleteUserAddressAsync(Guid id)
        {
            var userIdClaim = _httpContextAccessor.HttpContext?.User?.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userIdClaim))
                throw new LogicException("Auth", "User authentication required.");

            var currentUserId = Guid.Parse(userIdClaim);

            var address = await _userAddressRepository.GetByIdAsync(id);

            if (address == null || address.AppUserId != currentUserId)
                throw new LogicException("NotFound", "Address record not found or you don't have permission to delete it.");

            _userAddressRepository.Remove(address);

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

        public async Task TUpdateUserAddressAsync(UpdateUserAddressDto updateUserAddressDto)
        {
            var userIdClaim = _httpContextAccessor.HttpContext?.User?.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userIdClaim))
                throw new LogicException("Auth", "User session not found.");

            var currentUserId = Guid.Parse(userIdClaim);

            var existAddress = await _userAddressRepository.GetByIdAsync(updateUserAddressDto.Id);

            if (existAddress == null || existAddress.AppUserId != currentUserId)
                throw new LogicException("NotFound", "Address record not found or access denied.");

            bool isTitleExists = await _userAddressRepository.AnyAsync(x =>
                x.AppUserId == currentUserId &&
                x.Title.ToLower() == updateUserAddressDto.Title.ToLower() &&
                x.Id != updateUserAddressDto.Id);

            if (isTitleExists)
                throw new LogicException("Duplicate", $"You already have an address titled '{updateUserAddressDto.Title}'.");

            bool isAddressExists = await _userAddressRepository.AnyAsync(x =>
                x.AppUserId == currentUserId &&
                x.FullAddress.ToLower() == updateUserAddressDto.FullAddress.ToLower() &&
                x.Id != updateUserAddressDto.Id);

            if (isAddressExists)
                throw new LogicException("Duplicate", "This full address is already registered in your account.");

            _mapper.Map(updateUserAddressDto, existAddress);
            existAddress.UpdatedDate = DateTime.Now;
            existAddress.IsDeleted = updateUserAddressDto.IsDeleted;

            _userAddressRepository.Update(existAddress);
            await _uow.SaveAsync();
        }
    }
}
