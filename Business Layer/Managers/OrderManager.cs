using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;
using Core_Layer.Dtos.OrderDtos;
using Core_Layer.IRepositories;
using Core_Layer.IServices;
using Microsoft.EntityFrameworkCore;

namespace Business_Layer.Managers
{
    public class OrderManager : IOrderService
    {
        private readonly IOrderRepository _orderRepository;
        private readonly IMapper _mapper;

        public OrderManager(IOrderRepository orderRepository, IMapper mapper)
        {
            _orderRepository = orderRepository;
            _mapper = mapper;
        }

        public async Task<OrderListDto?> TGetOrderDetailsAsync(Guid orderId)
        {
            var order = await _orderRepository.Where(o => o.Id == orderId)
                .Include(o => o.OrderItems)
                    .ThenInclude(oi => oi.Product)
                .FirstOrDefaultAsync();

            return _mapper.Map<OrderListDto>(order);
        }

        public async Task<List<OrderListDto>> TGetOrdersByUserIdAsync(Guid userId)
        {
            var query = _orderRepository.Where(x => x.AppUserId == userId && !x.IsDeleted);
            var orders = await query
                .Include(o => o.OrderItems)
                    .ThenInclude(oi => oi.Product)
                        .ThenInclude(p => p.ProductImages)
                .OrderByDescending(o => o.CreatedDate) 
                .ToListAsync();

            return _mapper.Map<List<OrderListDto>>(orders);
        }
    }
}
