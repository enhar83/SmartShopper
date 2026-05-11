using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Core_Layer.Dtos.ChurnPredictionDtos;

namespace Core_Layer.IServices
{
    public interface ICustomerChurnResultService
    {
        Task<List<ChurnPredictionResultDto>> TProcessAllCustomersChurnAsync();
        Task<List<ChurnPredictionResultDto>> TGetAllChurnResultsAsync();
        Task<bool> TTrainChurnModelAsync();
    }
}
