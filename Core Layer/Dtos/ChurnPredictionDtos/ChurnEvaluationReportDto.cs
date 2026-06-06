using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core_Layer.Dtos.ChurnPredictionDtos
{
    public class ChurnEvaluationReportDto
    {
        public double Accuracy { get; set; }
        public double F1Score { get; set; }
        public double Precision { get; set; }
        public double Recall { get; set; }
        public double AreaUnderCurve { get; set; }
        public DateTime EvaluatedAt { get; set; }
    }
}
