using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Core_Layer.Dtos.CustomerSegmentationDtos;

namespace Core_Layer.IServices
{
    public interface ICustomerSegmentationService
    {
        Task<bool> TTrainModelAsync();
        Task TProcessBatchSegmentationAsync();
        Task<List<CustomerSegmentResultDto>> TGetSegmentationResultsAsync();
        Task<CustomerSegmentDto> TGetUserSegmentAsync(Guid userId);
    }
}
