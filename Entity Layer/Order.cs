using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Entity_Layer.Common;

namespace Entity_Layer
{
    public class Order:BaseEntity
    {
        public Guid AppUserId { get; set; }
        public virtual AppUser AppUser { get; set; } = null!;
        public decimal TotalPrice { get; set; }
        public OrderStatus Status { get; set; } = OrderStatus.Pending;
        public Guid AddressId { get; set; }
        public virtual UserAddress UserAddress { get; set; } = null!;
        public string DeliveryAddressSnapshot { get; set; } = null!;
        public ICollection<OrderItem> OrderItems { get; set; } = new List<OrderItem>();
    }
}

public enum OrderStatus
{
    Pending = 1,
    Confirmed = 2,
    Processing = 3,
    Shipped = 4,
    Delivered = 5,
    Cancelled = 6,
    Returned = 7
}
