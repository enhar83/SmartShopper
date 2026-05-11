using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.ML.Data;

namespace Business_Layer.MLModels.ChurnPredictionModels
{
    public class ChurnPredictionModelInput
    {
        [LoadColumn(0)]
        public float TotalSpend { get; set; }
        [LoadColumn(1)]
        public float OrderCount { get; set; }
        [LoadColumn(2)]
        public float DaysSinceLastOrder { get; set; }
        [LoadColumn(3)]
        public float AverageOrderValue { get; set; }
        [LoadColumn(4)]
        public bool Label { get; set; }
    }
}
