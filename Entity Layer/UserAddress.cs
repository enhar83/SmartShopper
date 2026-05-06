using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Entity_Layer.Common;

namespace Entity_Layer
{
    public class UserAddress:BaseEntity
    {
        public required string Title { get; set; }
        public required string Country { get; set; }
        public required string City { get; set; }
        public string? District { get; set; }
        public required string FullAddress { get; set; }
        public Guid AppUserId { get; set; }
        public virtual AppUser AppUser { get; set; } = null!;
        public virtual ICollection<Order> Orders { get; set; } = new List<Order>();
    }
}
