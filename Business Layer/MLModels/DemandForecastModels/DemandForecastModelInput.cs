using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.ML.Data;

namespace Business_Layer.MLModels.DemandForecastModels
{
    public class DemandForecastModelInput
    {
        [LoadColumn(0)]
        public string Country { get; set; } = null!;
        [LoadColumn(1)]
        public string City { get; set; } = null!;
        [LoadColumn(2)]
        public float Month { get; set; }
        [LoadColumn(3)]
        public float Year { get; set; }
        [LoadColumn(4)]
        [ColumnName("Label")]
        public float TotalRevenue { get; set; }
    }
}
