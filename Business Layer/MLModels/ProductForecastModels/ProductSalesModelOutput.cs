using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.ML.Data;

namespace Business_Layer.MLModels.ProductForecastModels
{
    public class ProductSalesModelOutput
    {
        [ColumnName("Score")]
        public float PredictedQuantity { get; set; }
    }
}
