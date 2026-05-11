using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.ML.Data;

namespace Business_Layer.MLModels.ProductForecastModels
{
    public class ProductSalesModelInput
    {
        [LoadColumn(0)]
        public string ProductId { get; set; } = null!;
        [LoadColumn(1)]
        public float Month { get; set; }
        [LoadColumn(2)]
        public float Year { get; set; }
        [LoadColumn(3)]
        public float AveragePrice { get; set; }
        [LoadColumn(4)]
        [ColumnName("Label")]
        public float TotalQuantitySold { get; set; }
    }
}
