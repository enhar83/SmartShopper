using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core_Layer.Dtos.DiscountDtos
{
    public class AssignDiscountDto
    {
        public Guid AppUserId { get; set; } 
        public Guid DiscountId { get; set; }
    }
}
