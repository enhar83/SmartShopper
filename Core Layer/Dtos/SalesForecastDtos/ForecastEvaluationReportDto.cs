using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core_Layer.Dtos.SalesForecastDtos
{
    public class ForecastEvaluationReportDto
    {
        public double MeanAbsoluteError { get; set; }
        public double RootMeanSquaredError { get; set; }
        public double MeanAbsolutePercentageError { get; set; }
        public DateTime EvaluatedAt { get; set; }
    }
}
