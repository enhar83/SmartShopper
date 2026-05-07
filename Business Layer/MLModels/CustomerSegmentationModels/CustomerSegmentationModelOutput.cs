using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.ML.Data;

namespace Business_Layer.MLModels.CustomerSegmentationModels
{
    public class CustomerSegmentationModelOutput
    {
        [ColumnName("PredictedLabel")]
        public string? Prediction { get; set; }
        public float[]? Score { get; set; }
    }
}

/*
    * Modelin verdiği ham cevaptır.
    * Prediction: Sistemin o kullanıcıyı hangi cluster içerisine dahil ettiğini gösterir.
    * Score: Bu bir dizidir, kullanıcının her bir kümeye ne kadar yakın olduğunun mesafesini verir. 
 
 */