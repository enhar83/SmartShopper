using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Core_Layer.Dtos.CartDtos;

namespace Core_Layer.Dtos.OrderDtos
{
    public class CheckoutSummaryDto
    {
        public List<CartItemDto> Items { get; set; } = new();
        public decimal SubTotal { get; set; } 
        public decimal TotalPrice { get; set; }

    }
}
