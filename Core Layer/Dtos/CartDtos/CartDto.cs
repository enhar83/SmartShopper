using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core_Layer.Dtos.CartDtos
{
    public class CartDto
    {
        public Guid Id { get; set; }
        public List<CartItemDto> Items { get; set; } = new List<CartItemDto>();
        public decimal GrandTotal => Items.Sum(x => x.TotalItemPrice);
        public int TotalQuantity => Items.Sum(x => x.Quantity);
    }
}
