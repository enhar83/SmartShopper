using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Entity_Layer.Common;

namespace Entity_Layer
{
    public class Order:BaseEntity
    {
        public DateTime OrderDate { get; set; }
        public Guid AppUserId { get; set; }
        public required AppUser AppUser { get; set; }

        public ICollection<OrderItem> OrderItems { get; set; } = new List<OrderItem>();
    }
}
