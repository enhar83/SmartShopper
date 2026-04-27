using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core_Layer.Dtos.RoleDtos
{
    public class AssignRoleDto
    {
        public Guid RoleId { get; set; }
        public required string RoleName { get; set; }
        public bool RoleExist { get; set; }
    }
}
