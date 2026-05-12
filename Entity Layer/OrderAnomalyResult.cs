using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Entity_Layer.Common;

namespace Entity_Layer
{
    public class OrderAnomalyResult:BaseEntity
    {
        public Guid OrderId { get; set; }
        public virtual Order Order { get; set; } = null!;
        public double Score { get; set; } 
        public double PValue { get; set; } 
        public int AnomalyTag { get; set; } 
        public bool IsAnomaly { get; set; }
        public string Description { get; set; } = null!;
    }
}
