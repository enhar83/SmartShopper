using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Entity_Layer.Common;

namespace Entity_Layer
{
    public class ProductSalesForecast:BaseEntity
    {
        public Guid ProductId { get; set; }
        public virtual Product Product { get; set; } = null!;
        public int TargetMonth { get; set; }
        public int TargetYear { get; set; }
        public int PredictedQuantity { get; set; } 
        public decimal ExpectedRevenue { get; set; } 
        public double ConfidenceScore { get; set; }
    }
}
