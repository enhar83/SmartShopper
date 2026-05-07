using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Business_Layer.MLModels.CustomerSegmentationModels
{
    public class CustomerSegmentationModelInput
    {
        public float TotalSpend { get; set; }
        public float OrderCount { get; set; }
        public float DaysSinceLastOrder { get; set; }
        public string? ActualSegment { get; set; }
    }
}

/*
    ML Modeli ile olan teknik kontratlardır:

    * Modelin eğitim sırasında neyi girdi olarak kabul edeceğini belirler. 
    * İçerisinde sadece float türdeki verilerin olma sebebi ise ML.NET'in sayısal verilerle çalışmasıdır.
    * ActualSegment alanı eğitimden sonra tahmin edilen sonucu karşılaştırmak için opsiyonel olarak tutulan bir proptur.
 
 */
