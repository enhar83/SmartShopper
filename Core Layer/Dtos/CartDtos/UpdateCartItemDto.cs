using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core_Layer.Dtos.CartDtos
{
    public class UpdateCartItemDto
    {
        public Guid Id { get; set; }
        public int Quantity { get; set; }
    }
}
