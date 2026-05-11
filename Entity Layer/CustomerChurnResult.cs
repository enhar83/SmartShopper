using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Entity_Layer.Common;

namespace Entity_Layer
{
    public class CustomerChurnResult : BaseEntity
    {
        public Guid AppUserId { get; set; }
        public virtual AppUser AppUser { get; set; } = null!;
        public bool IsChurn { get; set; }
        public decimal ChurnProbability { get; set; }
        public float Recency { get; set; }
        public float Frequency { get; set; }
        public decimal Monetary { get; set; }
        public DateTime LastUpdated { get; set; }
    }
}
