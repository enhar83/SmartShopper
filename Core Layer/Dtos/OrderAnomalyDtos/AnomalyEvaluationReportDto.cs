using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core_Layer.Dtos.OrderAnomalyDtos
{
    public class AnomalyEvaluationReportDto
    {
        public double ConfidenceLevel { get; set; }
        public int TotalAnalyzedOrders { get; set; }
        public int TotalAnomaliesFound { get; set; }
        public double AnomalyDetectionRate { get; set; } 
        public DateTime EvaluatedAt { get; set; }
    }
}
