using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core_Layer.Dtos.CustomerSegmentationDtos
{
    public class CustomerSegmentDto
    {
        public Guid AppUserId { get; set; }
        public required string UserFullName { get; set; }
        public required string SegmentLabel { get; set; } 
        public double ConfidenceScore { get; set; }
        public float Recency { get; set; }    
        public float Frequency { get; set; } 
        public float Monetary { get; set; } 
        public DateTime LastUpdated { get; set; }
    }
}
