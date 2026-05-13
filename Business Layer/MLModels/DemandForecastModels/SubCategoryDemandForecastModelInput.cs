using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.ML.Data;

namespace Business_Layer.MLModels.DemandForecastModels
{
    public class SubCategoryDemandForecastModelInput
    {
        [LoadColumn(0)]
        public string CategoryName { get; set; } = null!;

        [LoadColumn(1)]
        public string SubCategoryName { get; set; } = null!;

        [LoadColumn(2)]
        public float Year { get; set; }

        [LoadColumn(3)]
        public float Month { get; set; }

        [LoadColumn(4)]
        public float SubCategoryAOV { get; set; } 

        [LoadColumn(5)]
        [ColumnName("Label")] 
        public float Label { get; set; }
    }
}
