using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core_Layer.Dtos.RoleDtos
{
    public class UsersInRoleDto
    {
        public Guid Id { get; set; }
        public required string FullName { get; set; }
        public required string Email { get; set; }
        public required string UserName { get; set; }
    }
}
