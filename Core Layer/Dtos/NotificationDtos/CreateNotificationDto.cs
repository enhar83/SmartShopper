using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core_Layer.Dtos.NotificationDtos
{
    public class CreateNotificationDto
    {
        public Guid AppUserId { get; set; }
        public string Title { get; set; } = null!;
        public string Message { get; set; } = null!;
        public string NotificationType { get; set; } = null!;
        public string? RelatedUrl { get; set; }
    }
}
