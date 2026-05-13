using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core_Layer.Dtos.DemandForecastDtos
{
    public class SubCategoryDemandForecastDto
    {
        public Guid Id { get; set; }
        public Guid SubCategoryId { get; set; }
        public string SubCategoryName { get; set; } = null!;
        public string CategoryName { get; set; } = null!; 
        public int TargetMonth { get; set; }
        public int TargetYear { get; set; }
        public int PredictedSalesCount { get; set; }
        public decimal PredictedRevenue { get; set; }
        public double ModelAccuracyScore { get; set; }
        public string Period => $"{TargetMonth:D2}/{TargetYear}";
        public string PotentialLevel => PredictedRevenue switch
        {
            > 100000 => "High Growth",
            > 40000 => "Stable",
            _ => "Emerging"
        };
    }
}
