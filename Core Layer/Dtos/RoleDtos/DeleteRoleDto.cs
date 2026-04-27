using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core_Layer.Dtos.RoleDtos
{
    public class DeleteRoleDto
    {
        public Guid Id { get; set; }
        public required string RoleName { get; set; }
    }
}
