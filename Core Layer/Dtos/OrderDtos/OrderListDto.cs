using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core_Layer.Dtos.OrderDtos
{
    public class OrderListDto
    {
        public Guid Id { get; set; }
        public DateTime CreatedDate { get; set; } 
        public decimal TotalPrice { get; set; }
        public OrderStatus Status { get; set; }
        public string StatusDescription => Status.ToString(); 
        public string DeliveryAddressSnapshot { get; set; } = null!;
        public decimal? SubTotal { get; set; }
        public decimal? DiscountAmount { get; set; }
        public string? AppliedDiscountName { get; set; }
        public List<OrderItemListDto> OrderItems { get; set; } = new();
    }
}
