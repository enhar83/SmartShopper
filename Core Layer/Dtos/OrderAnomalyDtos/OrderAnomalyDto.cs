using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core_Layer.Dtos.OrderAnomalyDtos
{
    public class OrderAnomalyDto
    {
        public Guid Id { get; set; }
        public Guid OrderId { get; set; }
        public string CustomerName { get; set; } = null!;
        public decimal OrderTotal { get; set; }
        public DateTime OrderDate { get; set; }
        public double Score { get; set; }
        public double PValue { get; set; }
        public bool IsAnomaly { get; set; }
        public string Description { get; set; } = null!;
        public double RiskPercentage => Math.Round((1 - PValue) * 100, 2);
        public string RiskBadgeClass => IsAnomaly ? "danger" : "success";
    }
}
