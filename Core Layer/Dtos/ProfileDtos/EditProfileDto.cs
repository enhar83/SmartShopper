using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace Core_Layer.Dtos.ProfileDtos
{
    public class EditProfileDto
    {
        public Guid Id { get; set; }
        public required string Name { get; set; }
        public required string Surname { get; set; }
        public string? City { get; set; }
        public IFormFile? UserImage { get; set; }
        public required string UserName { get; set; }
        public string? PhoneNumber { get; set; }
    }
}
