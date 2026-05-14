using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Entity_Layer.Common;

namespace Entity_Layer
{
    public class DiscountCustomer:BaseEntity
    {
        public Guid DiscountId { get; set; }
        public virtual Discount Discount { get; set; } = null!;
        public Guid AppUserId { get; set; }
        public virtual AppUser AppUser { get; set; } = null!;
        public bool IsUsed { get; set; }
        public DateTime? UsedDate { get; set; }
    }
}
