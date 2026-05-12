using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.ML.Data;

namespace Business_Layer.MLModels.RecommenderModels
{
    public class RecommendationModelInput
    {
        [LoadColumn(0)]
        public float UserId { get; set; }
        [LoadColumn(1)]
        public float ProductId { get; set; }
        [LoadColumn(2)]
        public float Label { get; set; }
    }
}
