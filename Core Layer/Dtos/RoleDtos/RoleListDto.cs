using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core_Layer.Dtos.RoleDtos
{
    public class RoleListDto
    {
        public Guid Id { get; set; }
        public required string RoleName { get; set; }
        public required string Description { get; set; }
        public bool IsDeleted { get; set; }
        public DateTime CreatedDate { get; set; }
    }
}
