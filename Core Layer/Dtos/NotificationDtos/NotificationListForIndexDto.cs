using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core_Layer.Dtos.NotificationDtos
{
    public class NotificationListForIndexDto
    {
        public Guid Id { get; set; }
        public string Title { get; set; } = null!;
        public string Message { get; set; } = null!;
        public bool IsRead { get; set; }
        public string NotificationType { get; set; } = null!;
        public string? RelatedUrl { get; set; }
        public DateTime CreatedDate { get; set; }
    }
}
