using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Core_Layer.Dtos.ProductForecastDtos;

namespace Core_Layer.IServices
{
    public interface IProductSalesForecastService
    {
        Task<List<ProductSalesForecastDto>> TGetAllForecastsAsync();
        Task<bool> TTrainAndGenerateForecastsAsync();
    }
}
