using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core_Layer.Dtos.UserAddressDtos
{
    public class UserAddressListForCheckoutDto
    {
        public Guid Id { get; set; }
        public required string Title { get; set; }
        public required string FullAddress { get; set; }
    }
}
