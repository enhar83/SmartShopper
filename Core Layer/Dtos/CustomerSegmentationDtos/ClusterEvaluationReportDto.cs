using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core_Layer.Dtos.CustomerSegmentationDtos
{
    public class ClusterEvaluationReportDto
    {
        public double DaviesBouldinIndex { get; set; }
        public double AverageDistance { get; set; }
        public double ClusterQualityPercentage { get; set; }
        public DateTime EvaluatedAt { get; set; }
    }
}
