using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Core_Layer.Dtos.DiscountDtos;
using Core_Layer.Dtos.NotificationDtos;
using Core_Layer.Exceptions;
using Core_Layer.IRepositories;
using Core_Layer.IServices;
using Entity_Layer;
using Microsoft.EntityFrameworkCore;

namespace Business_Layer.Managers
{
    public class DiscountManager : IDiscountService
    {
        private readonly IDiscountRepository _discountRepository;
        private readonly IDiscountCustomerRepository _discountCustomerRepository;
        private readonly INotificationService _notificationService;
        private readonly IUnitOfWork _uow;
        private readonly IMapper _mapper;

        public DiscountManager(IDiscountRepository discountRepository, IDiscountCustomerRepository discountCustomerRepository, INotificationService notificationService, IUnitOfWork uow, IMapper mapper)
        {
            _discountRepository = discountRepository;
            _discountCustomerRepository = discountCustomerRepository;
            _notificationService = notificationService;
            _uow = uow;
            _mapper = mapper;
        }

        public async Task TCreateDiscountAsync(DiscountCreateDto createDto)
        {
            bool isDiscountNameExists = await _discountRepository.AnyAsync(x => x.Name == createDto.Name);

            if (isDiscountNameExists)
                throw new LogicException("Name", "A discount campaign with this name already exists. Please choose a different name.");

            var discountEntity = _mapper.Map<Discount>(createDto);
            discountEntity.CreatedDate = DateTime.Now;
            discountEntity.IsDeleted = false;

            await _discountRepository.AddAsync(discountEntity);
            await _uow.SaveAsync();
        }

        public async Task<List<DiscountListDto>> TGetAllDiscountsAsync()
        {
            var discounts = await _discountRepository.GetAll().ToListAsync();
            return _mapper.Map<List<DiscountListDto>>(discounts);
        }

        public async Task<DiscountUpdateDto> TGetDiscountForUpdateAsync(Guid id)
        {
            var discount = await _discountRepository.GetByIdAsync(id);
            if (discount == null)
                throw new LogicException("NotFound", "No discount campaign was found to update.");

            return _mapper.Map<DiscountUpdateDto>(discount);
        }

        public async Task TUpdateDiscountAsync(DiscountUpdateDto updateDto)
        {
            bool isDiscountNameExists = await _discountRepository.AnyAsync(x => x.Name == updateDto.Name && x.Id != updateDto.Id);

            if (isDiscountNameExists)
                throw new LogicException("Name", "There is already another discount campaign with this name. Please choose a different name.");

            var existingDiscount = await _discountRepository.GetByIdAsync(updateDto.Id);
            if (existingDiscount == null)
                throw new LogicException("NotFound", "No discount campaign was found to update.");

            _mapper.Map(updateDto, existingDiscount);
            existingDiscount.UpdatedDate = DateTime.Now;

            _discountRepository.Update(existingDiscount);
            await _uow.SaveAsync();
        }

        public async Task TDeleteDiscountAsync(Guid id)
        {
            var discount = await _discountRepository.GetByIdAsync(id);
            if (discount == null)
                throw new LogicException("NotFound", "No campaign was found to delete.");

            discount.IsDeleted = true;
            discount.UpdatedDate = DateTime.Now;
            _discountRepository.Update(discount);
            await _uow.SaveAsync();
        }

        public async Task TAssignDiscountToUserAsync(AssignDiscountDto assignDto)
        {
            bool alreadyExists = await _discountCustomerRepository.AnyAsync(x =>
                x.AppUserId == assignDto.AppUserId &&
                x.DiscountId == assignDto.DiscountId &&
                !x.IsUsed);

            if (alreadyExists)
                throw new LogicException("Duplicate", "This discount has already been applied to this user and has not yet been used.");

            var assignment = new DiscountCustomer
            {
                AppUserId = assignDto.AppUserId,
                DiscountId = assignDto.DiscountId,
                IsUsed = false,
                CreatedDate = DateTime.Now
            };

            await _discountCustomerRepository.AddAsync(assignment);

            var discountDetails = await _discountRepository.GetByIdAsync(assignDto.DiscountId);

            if (discountDetails != null)
            {
                var notificationDto = new CreateNotificationDto
                {
                    AppUserId = assignDto.AppUserId,
                    Title = "Exclusive Discount Unlocked!",
                    Message = $"Congratulations! A special '{discountDetails.Name}' discount has been added to your account. Don't miss this opportunity!",
                    NotificationType = "Discount",
                    RelatedUrl = "Discount/Index" 
                };

                await _notificationService.TAddAsync(notificationDto);
            }

            await _uow.SaveAsync();
        }

        public async Task<List<DiscountAssignedUserDto>> TGetUsersByDiscountIdAsync(Guid discountId)
        {
            var assignments = await _discountCustomerRepository.GetAll()
                .Where(x => x.DiscountId == discountId && !x.IsDeleted)
                .Select(x => new DiscountAssignedUserDto
                {
                    AssignmentId = x.Id,
                    UserFullName = x.AppUser.Name + " " + x.AppUser.Surname,
                    Email = x.AppUser.Email!,
                    IsUsed = x.IsUsed,
                    AssignedDate = x.CreatedDate
                }).ToListAsync();

            return assignments;
        }

        public async Task TRemoveDiscountFromUserAsync(Guid assignmentId)
        {
            var assignment = await _discountCustomerRepository.GetByIdAsync(assignmentId);
            if (assignment == null)
                throw new LogicException("NotFound", "No appointment record found.");

            if (assignment.IsUsed)
                throw new LogicException("Used", "A used discount cannot be reclaimed.");

            assignment.IsDeleted = true;
            _discountCustomerRepository.Update(assignment);
            await _uow.SaveAsync();
        }

        public async Task<List<UserDiscountListDto>> TGetUserSpecificDiscountsAsync(Guid userId)
        {
            var userAssignments = await _discountCustomerRepository.GetAll()
                .Include(x => x.Discount)
                .Where(x => x.AppUserId == userId && !x.IsDeleted)
                .OrderByDescending(x => x.CreatedDate)
                .ToListAsync();

            return _mapper.Map<List<UserDiscountListDto>>(userAssignments);
        }

        public async Task<List<UserDiscountListDto>> TGetAvailableDiscountsForCheckoutAsync(Guid userId)
        {
            var availableAssignments = await _discountCustomerRepository.GetAll()
                .Include(x => x.Discount)
                .Where(x => x.AppUserId == userId && !x.IsDeleted)
                .ToListAsync();

            return _mapper.Map<List<UserDiscountListDto>>(availableAssignments);
        }

        public async Task TMarkDiscountAsUsedAsync(Guid userId, Guid assignmentId)
        {
            var existingAssignment = await _discountCustomerRepository.GetAll()
                .FirstOrDefaultAsync(x => x.AppUserId == userId && x.Id == assignmentId && !x.IsUsed);

            if (existingAssignment == null)
                throw new LogicException("NotFound", "No available discount definition was found.");

            existingAssignment.IsUsed = true;
            existingAssignment.UpdatedDate = DateTime.Now;

            _discountCustomerRepository.Update(existingAssignment);
            await _uow.SaveAsync();
        }

        public async Task TAssignDiscountToMultipleUsersAsync(Guid discountId, List<Guid> userIds)
        {
            if (userIds == null || !userIds.Any()) return;

            var existingAssignments = await _discountCustomerRepository.GetAll()
                .Where(x => x.DiscountId == discountId && !x.IsUsed && !x.IsDeleted && userIds.Contains(x.AppUserId))
                .Select(x => x.AppUserId)
                .ToListAsync();

            var targetUserIds = userIds.Except(existingAssignments).ToList();

            if (!targetUserIds.Any()) return; 

            var newAssignments = targetUserIds.Select(userId => new DiscountCustomer
            {
                AppUserId = userId,
                DiscountId = discountId,
                IsUsed = false,
                CreatedDate = DateTime.Now
            }).ToList();

            await _discountCustomerRepository.AddRangeAsync(newAssignments);

            var discountDetails = await _discountRepository.GetByIdAsync(discountId);
            if (discountDetails != null)
            {
                foreach (var userId in targetUserIds)
                {
                    var notificationDto = new CreateNotificationDto
                    {
                        AppUserId = userId,
                        Title = "You've earned a special discount!",
                        Message = $"Congratulations! A special discount of '{discountDetails.Name}' has been added to your account. Don't miss out on the opportunity!",
                        NotificationType = "Discount",
                        RelatedUrl = "Discount/Index"
                    };

                    await _notificationService.TAddAsync(notificationDto);
                }
            }

            await _uow.SaveAsync();
        }
    }
}