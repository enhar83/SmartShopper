using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Core_Layer.Dtos.NotificationDtos;
using Core_Layer.Exceptions;
using Core_Layer.IRepositories;
using Core_Layer.IServices;
using Entity_Layer;
using Microsoft.EntityFrameworkCore;

namespace Business_Layer.Managers
{
    public class NotificationManager : INotificationService
    {
        private readonly INotificationRepository _notificationRepository;
        private readonly IUnitOfWork _uow;
        private readonly IMapper _mapper;

        public NotificationManager(INotificationRepository notificationRepository, IUnitOfWork uow, IMapper mapper)
        {
            _notificationRepository = notificationRepository;
            _uow = uow;
            _mapper = mapper;
        }

        public async Task TAddAsync(CreateNotificationDto createDto)
        {
            var notification = _mapper.Map<Notification>(createDto);

            notification.CreatedDate = DateTime.Now;
            notification.IsRead = false;
            notification.IsDeleted = false;

            await _notificationRepository.AddAsync(notification);
            await _uow.SaveAsync();
        }

        public async Task<List<NotificationListDto>> TGetUserNotificationsAsync(Guid userId)
        {
            var notifications = await _notificationRepository.GetAll()
                .Where(x => x.AppUserId == userId && !x.IsDeleted)
                .OrderByDescending(x => x.CreatedDate)
                .Take(5)
                .ToListAsync();

            return _mapper.Map<List<NotificationListDto>>(notifications);
        }

        public async Task<int> TGetUnreadNotificationCountAsync(Guid userId)
        {
            var unreadCount = await _notificationRepository.GetAll()
                .CountAsync(x => x.AppUserId == userId && !x.IsRead && !x.IsDeleted);

            return unreadCount;
        }

        public async Task TMarkAsReadAsync(Guid notificationId)
        {
            var notification = await _notificationRepository.GetByIdAsync(notificationId);

            if (notification == null)
                throw new LogicException("NotFound", "No notification found to update.");

            notification.IsRead = true;
            notification.UpdatedDate = DateTime.Now;

            _notificationRepository.Update(notification);
            await _uow.SaveAsync();
        }

        public async Task TMarkAllAsReadAsync(Guid userId)
        {
            var unreadNotifications = await _notificationRepository.GetAll()
                .Where(x => x.AppUserId == userId && !x.IsRead && !x.IsDeleted)
                .ToListAsync();

            if (unreadNotifications.Any())
            {
                foreach (var notification in unreadNotifications)
                {
                    notification.IsRead = true;
                    notification.UpdatedDate = DateTime.Now;
                    _notificationRepository.Update(notification);
                }
                await _uow.SaveAsync();
            }
        }

        public async Task TDeleteNotificationAsync(Guid id)
        {
            var notification = await _notificationRepository.GetByIdAsync(id);

            if (notification == null)
                throw new LogicException("NotFound", "No notification found to delete.");

            notification.IsDeleted = true;
            notification.UpdatedDate = DateTime.Now;

            _notificationRepository.Update(notification);
            await _uow.SaveAsync();
        }

        public async Task<List<NotificationListForIndexDto>> TGetNotificationHistoryAsync(Guid userId)
        {
            var notifications = await _notificationRepository.GetAll()
                .Where(x => x.AppUserId == userId && !x.IsDeleted)
                .OrderByDescending(x => x.CreatedDate)
                .ToListAsync();

            return _mapper.Map<List<NotificationListForIndexDto>>(notifications);
        }
    }
}