using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Entity_Layer.Common;

namespace Entity_Layer
{
    public class Notification : BaseEntity
    {
        public Guid AppUserId { get; set; }
        public AppUser AppUser { get; set; } = null!;
        public string Title { get; set; } = null!;
        public string Message { get; set; } = null!;
        public bool IsRead { get; set; }
        public string NotificationType { get; set; } = null!;
        public string? RelatedUrl { get; set; }
    }
}
