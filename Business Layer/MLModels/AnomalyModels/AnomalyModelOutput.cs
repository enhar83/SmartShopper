using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.ML.Data;

namespace Business_Layer.MLModels.AnomalyModels
{
    public class AnomalyModelOutput
    {
        [VectorType(3)]
        public double[] Prediction { get; set; } = null!;
    }
}
