using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core_Layer.Dtos.ChurnPredictionDtos
{
    public class ChurnPredictionResultDto
    {
        public Guid AppUserId { get; set; }
        public string UserFullName { get; set; } = null!;
        public bool IsChurn { get; set; }
        public double ChurnProbability { get; set; }
        public float Recency { get; set; }
        public float Frequency { get; set; }
        public float Monetary { get; set; }
        public DateTime LastUpdated { get; set; }
        public string RiskLevel => ChurnProbability switch
        {
            > 80 => "High Risk",
            > 50 => "Medium Risk",
            _ => "Low Risk"
        };
    }
}
