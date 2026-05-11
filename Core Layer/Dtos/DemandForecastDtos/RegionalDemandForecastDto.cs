using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core_Layer.Dtos.DemandForecastDtos
{
    public class RegionalDemandForecastDto
    {
        public Guid Id { get; set; }
        public string Country { get; set; } = null!;
        public string City { get; set; } = null!;
        public int TargetMonth { get; set; }
        public int TargetYear { get; set; }
        public int PredictedOrderCount { get; set; }
        public decimal PredictedRevenue { get; set; }
        public double ModelAccuracyScore { get; set; }
        public DateTime UpdatedDate { get; set; }
        public string Period => $"{TargetMonth:D2}/{TargetYear}";
        public string Location => $"{City}, {Country}";
        public string RevenueLevel => PredictedRevenue switch
        {
            > 50000 => "High Potential",
            > 20000 => "Medium Potential",
            _ => "Emerging Market"
        };
    }
}
