using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core_Layer.Dtos.CartDtos
{
    public class CartItemDto
    {
        public Guid Id { get; set; } 
        public Guid ProductId { get; set; }
        public required string ProductName { get; set; }
        public string? ProductImageUrl { get; set; } 
        public decimal Price { get; set; }
        public int Quantity { get; set; }
        public decimal TotalItemPrice => Price * Quantity;
    }
}
