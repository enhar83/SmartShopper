using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core_Layer.Dtos.JwtDtos
{
    public class JwtDto
    {
        public required string Name { get; set; }
        public required string Surname { get; set; }
        public required string UserName { get; set; }
        public required string Email { get; set; }
        public required List<string> Roles { get; set; }
        public required string Token { get; set; }
    }
}
