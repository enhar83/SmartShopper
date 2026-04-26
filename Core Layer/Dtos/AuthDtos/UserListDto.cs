using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core_Layer.Dtos.AuthDtos
{
    public class UserListDto
    {
        public required string FullName { get; set; }
        public required string UserName { get; set; }
        public required string Email { get; set; }
        public DateTime CreatedDate { get; set; }
        public bool EmailConfirmed { get; set; }
        public bool IsDeleted { get; set; }
    }
}
