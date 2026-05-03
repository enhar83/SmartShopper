using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;

namespace Entity_Layer
{
    public class AppUser:IdentityUser<Guid>
    {
        public required string Name { get; set; }
        public required string Surname { get; set; }
        public string? ImageUrl { get; set; }
        public bool IsDeleted { get; set; }
        public string? ActivationCode { get; set; }
        public DateTime CreatedDate { get; set; } = DateTime.Now;
        public ICollection<UserAddress>? Addresses { get; set; } 
        public ICollection<Favorite>? Favorites { get; set; }

    }
}
