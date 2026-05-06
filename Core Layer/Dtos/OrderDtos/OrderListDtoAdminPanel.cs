using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core_Layer.Dtos.OrderDtos
{
    public class OrderListDtoAdminPanel
    {
        public Guid Id { get; set; }
        public string OrderReference => Id.ToString().Substring(0, 8).ToUpper();
        public string CustomerFullName { get; set; } = null!;
        public string CustomerEmail { get; set; } = null!;
        public DateTime CreatedDate { get; set; }
        public decimal TotalPrice { get; set; }
        public OrderStatus Status { get; set; } 
        public string StatusDescription => Status.ToString();
        public int ItemCount { get; set; }
    }
}
