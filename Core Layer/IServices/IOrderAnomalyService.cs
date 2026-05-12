using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Core_Layer.Dtos.OrderAnomalyDtos;

namespace Core_Layer.IServices
{
    public interface IOrderAnomalyService
    {
        Task<List<OrderAnomalyDto>> TGetAllAnomaliesAsync();
        Task<bool> TRunAnomalyDetectionAsync();
        Task<List<CustomerOrderHistoryDto>> TGetCustomerOrderHistoryAsync(Guid orderId);
    }
}
