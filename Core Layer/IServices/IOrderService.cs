using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Core_Layer.Dtos.OrderDtos;

namespace Core_Layer.IServices
{
    public interface IOrderService
    {
        Task<List<OrderListDto>> TGetOrdersByUserIdAsync(Guid userId);
        Task<OrderListDto?> TGetOrderDetailsAsync(Guid orderId);
    }
}
