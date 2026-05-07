using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Entity_Layer.Common;

namespace Entity_Layer
{
    public class CustomerSegmentationResult:BaseEntity
    {
        public Guid AppUserId { get; set; }
        public required string SegmentLabel { get; set; } 
        public double ConfidenceScore { get; set; } 
        public DateTime LastUpdated { get; set; }
        public virtual AppUser AppUser { get; set; } = null!;
    }
}
