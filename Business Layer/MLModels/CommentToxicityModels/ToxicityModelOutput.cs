using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.ML.Data;

namespace Business_Layer.MLModels.CommentToxicityModels
{
    public class ToxicityModelOutput
    {
        [ColumnName("PredictedLabel")]
        public bool Prediction { get; set; }
        [ColumnName("Probability")]
        public float Probability { get; set; }
    }
}
