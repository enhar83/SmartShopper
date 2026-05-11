using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Core_Layer.Dtos.DemandForecastDtos;

namespace Core_Layer.IServices
{
    public interface IRegionalDemandForecastService
    {
        Task<List<RegionalDemandForecastDto>> TGetAllForecastsAsync();
        Task<bool> TTrainAndGenerateForecastsAsync();
    }
}
