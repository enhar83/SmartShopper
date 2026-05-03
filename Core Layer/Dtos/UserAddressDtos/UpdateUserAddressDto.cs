using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core_Layer.Dtos.UserAddressDtos
{
    public class UpdateUserAddressDto
    {
        public Guid Id { get; set; }
        public required string Title { get; set; }
        public required string Country { get; set; }
        public required string City { get; set; }
        public string? District { get; set; }
        public required string FullAddress { get; set; }
        public bool IsDeleted { get; set; }
    }
}
