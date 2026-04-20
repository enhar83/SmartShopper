using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Core_Layer.Entities;

namespace Entity_Layer
{
    public class OrderItem:BaseEntity
    {
        public Guid OrderId { get; set; }
        public required Order Order { get; set; }
        public Guid ProductId { get; set; }
        public required Product Product { get; set; }
        public int Quantity { get; set; }
        public decimal PriceAtPurchase { get; set; }
    }
}
