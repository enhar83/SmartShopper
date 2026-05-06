using AutoMapper;
using Core_Layer.Dtos.OrderDtos;
using Core_Layer.IRepositories;
using Core_Layer.IServices;
using Entity_Layer;
using Microsoft.EntityFrameworkCore;

namespace Business_Layer.Managers
{
    public class OrderManager : IOrderService
    {
        private readonly IOrderRepository _orderRepository;
        private readonly IProductRepository _productRepository;
        private readonly IUnitOfWork _uow;
        private readonly IMapper _mapper;

        public OrderManager(IOrderRepository orderRepository, IProductRepository productRepository, IUnitOfWork uow, IMapper mapper)
        {
            _orderRepository = orderRepository;
            _productRepository = productRepository;
            _uow = uow;
            _mapper = mapper;
        }

        public async Task<OrderListDto?> TGetOrderDetailsAsync(Guid orderId)
        {
            var order = await _orderRepository.Where(o => o.Id == orderId)
                .AsNoTracking()
                .Include(o => o.OrderItems)
                    .ThenInclude(oi => oi.Product)
                        .ThenInclude(p => p.ProductImages)
                .FirstOrDefaultAsync();

            return _mapper.Map<OrderListDto>(order);
        }

        public async Task<List<OrderListDto>> TGetOrdersByUserIdAsync(Guid userId)
        {
            var orders = await _orderRepository.Where(x => x.AppUserId == userId && !x.IsDeleted)
                .AsNoTracking()
                .Include(o => o.OrderItems)
                    .ThenInclude(oi => oi.Product)
                        .ThenInclude(p => p.ProductImages)
                .OrderByDescending(o => o.CreatedDate)
                .ToListAsync();

            return _mapper.Map<List<OrderListDto>>(orders);
        }

        public async Task<List<OrderListDtoAdminPanel>> TGetOrdersForAdminAsync()
        {
            var orders = await _orderRepository.GetAll()
                .AsNoTracking()
                .Include(x => x.AppUser)
                .Include(x => x.OrderItems)
                .OrderByDescending(x => x.CreatedDate)
                .ToListAsync();

            return _mapper.Map<List<OrderListDtoAdminPanel>>(orders);
        }

        public async Task<bool> TUpdateOrderStatusAsync(OrderStatusUpdateDto updateDto)
        {
            var order = await _orderRepository.Where(x => x.Id == updateDto.OrderId)
                .Include(x => x.OrderItems)
                .FirstOrDefaultAsync();

            if (order == null) return false;
            if (order.Status == updateDto.NewStatus) return true;

            try
            {
                if (updateDto.NewStatus == OrderStatus.Cancelled && order.Status != OrderStatus.Cancelled)
                {
                    foreach (var item in order.OrderItems)
                    {
                        var product = await _productRepository.GetByIdAsync(item.ProductId);
                        if (product != null)
                        {
                            product.Stock += item.Quantity;
                            _productRepository.Update(product);
                        }
                    }
                }

                order.Status = updateDto.NewStatus;
                _orderRepository.Update(order);

                await _uow.SaveAsync();
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }
    }
}