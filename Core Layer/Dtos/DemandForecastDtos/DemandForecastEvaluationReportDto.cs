using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core_Layer.Dtos.DemandForecastDtos
{
    public class DemandForecastEvaluationReportDto
    {
        public double RSquaredPercentage { get; set; }
        public double MeanAbsoluteError { get; set; }
        public double RootMeanSquaredError { get; set; }
        public DateTime EvaluatedAt { get; set; }
    }
}
