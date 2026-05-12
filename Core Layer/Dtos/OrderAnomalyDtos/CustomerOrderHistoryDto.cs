using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core_Layer.Dtos.OrderAnomalyDtos
{
    public class CustomerOrderHistoryDto
    {
        public Guid OrderId { get; set; }
        public DateTime OrderDate { get; set; }
        public decimal TotalPrice { get; set; }
        public string OrderDateString => OrderDate.ToString("dd.MM.yyyy HH:mm");
    }
}
