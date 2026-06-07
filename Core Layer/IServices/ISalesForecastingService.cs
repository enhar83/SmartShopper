using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Core_Layer.Dtos.SalesForecastDtos;

namespace Core_Layer.IServices
{
    public interface ISalesForecastingService
    {
        Task<List<SalesForecastResultDto>> TGetSalesForecastAsync(int horizonMonths = 12);
        Task<bool> TTrainForecastModelAsync();
        Task<ForecastEvaluationReportDto> TGetForecastMetricsAsync();
    }
}
