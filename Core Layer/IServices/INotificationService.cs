using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Core_Layer.Dtos.NotificationDtos;

namespace Core_Layer.IServices
{
    public interface INotificationService
    {
        Task TAddAsync(CreateNotificationDto createDto);
        Task<List<NotificationListDto>> TGetUserNotificationsAsync(Guid userId);
        Task<int> TGetUnreadNotificationCountAsync(Guid userId);
        Task TMarkAsReadAsync(Guid notificationId);
        Task TMarkAllAsReadAsync(Guid userId);
        Task TDeleteNotificationAsync(Guid id);
    }
}
