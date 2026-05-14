using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core_Layer.Dtos.DiscountDtos
{
    public class DiscountAssignedUserDto
    {
        public Guid AssignmentId { get; set; }
        public string UserFullName { get; set; } = null!;
        public string Email { get; set; } = null!;
        public bool IsUsed { get; set; } 
        public DateTime AssignedDate { get; set; }
    }
}
