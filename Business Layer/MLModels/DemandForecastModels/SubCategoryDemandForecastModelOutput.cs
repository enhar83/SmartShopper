using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.ML.Data;

namespace Business_Layer.MLModels.DemandForecastModels
{
    public class SubCategoryDemandForecastModelOutput
    {
        [ColumnName("Score")]
        public float PredictedCount { get; set; }
    }
}
