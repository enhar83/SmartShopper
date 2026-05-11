using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Entity_Layer.Common;

namespace Entity_Layer
{
    public class RegionalDemandForecast:BaseEntity
    {
            public string Country { get; set; } = null!;
            public string City { get; set; } = null!;
            public int TargetMonth { get; set; }
            public int TargetYear { get; set; }
            public int PredictedOrderCount { get; set; } 
            public decimal PredictedRevenue { get; set; } 
            public double ModelAccuracyScore { get; set; }
    }
}
