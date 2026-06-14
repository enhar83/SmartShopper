using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core_Layer.Dtos.ProductForecastDtos
{
    public class ProductSalesForecastDto
    {
        public Guid Id { get; set; }
        public Guid ProductId { get; set; }
        public string ProductName { get; set; } = null!;
        public decimal CurrentPrice { get; set; }
        public int TargetMonth { get; set; }
        public int TargetYear { get; set; }
        public int PredictedQuantity { get; set; }
        public decimal ExpectedRevenue { get; set; }
        public DateTime UpdatedDate { get; set; }
        public string Period => $"{TargetMonth:D2}/{TargetYear}";

        public string StockAction => PredictedQuantity switch
        {
            > 100 => "Urgent Restock (High Demand)",
            > 20 => "Normal Restock",
            > 0 => "Monitor Inventory",
            _ => "No Action Needed (Low Demand)"
        };
    }
}
