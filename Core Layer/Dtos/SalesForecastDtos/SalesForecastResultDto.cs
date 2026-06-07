using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core_Layer.Dtos.SalesForecastDtos
{
    public class SalesForecastResultDto
    {
        public string Period { get; set; } = null!; 
        public double ActualAmount { get; set; }
        public double ForecastAmount { get; set; }
        public double LowerBound { get; set; }
        public double UpperBound { get; set; }
        public bool IsForecast { get; set; }
    }
}
