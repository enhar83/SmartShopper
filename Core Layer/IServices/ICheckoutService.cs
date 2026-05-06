using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Core_Layer.Dtos.OrderDtos;

namespace Core_Layer.IServices
{
    public interface ICheckoutService
    {
        Task<CheckoutSummaryDto> TGetCheckoutSummaryAsync(Guid userId);
        Task<bool> TPlaceOrderAsync(Guid userId, Guid addressId);
    }
}
