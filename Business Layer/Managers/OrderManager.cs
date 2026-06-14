using AutoMapper;
using Core_Layer.Dtos.CommonDtos;
using Core_Layer.Dtos.NotificationDtos;
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
        private readonly INotificationService _notificationService;
        private readonly IUnitOfWork _uow;
        private readonly IMapper _mapper;

        public OrderManager(IOrderRepository orderRepository, IProductRepository productRepository, INotificationService notificationService, IUnitOfWork uow, IMapper mapper)
        {
            _orderRepository = orderRepository;
            _productRepository = productRepository;
            _notificationService = notificationService;
            _uow = uow;
            _mapper = mapper;
        }

        public async Task<OrderListDto?> TGetOrderDetailsAsync(Guid orderId)
        {
            var order = await _orderRepository.Where(o => o.Id == orderId)
                .AsNoTracking()
                .Include(o => o.AppliedDiscount) 
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
                .Include(o => o.AppliedDiscount)
                .Include(o => o.OrderItems)
                    .ThenInclude(oi => oi.Product)
                        .ThenInclude(p => p.ProductImages)
                .OrderByDescending(o => o.CreatedDate)
                .ToListAsync();

            return _mapper.Map<List<OrderListDto>>(orders);
        }

        public async Task<PaginatedResultDto<OrderListDtoAdminPanel>> TGetOrdersForAdminPaginatedAsync(int pageNumber = 1, int pageSize = 10)
        {
            var query = _orderRepository.GetAll()
                .AsNoTracking()
                .Include(x => x.AppUser)
                .Include(x => x.AppliedDiscount)
                .Include(x => x.OrderItems)
                .OrderByDescending(x => x.CreatedDate);

            var totalCount = await query.CountAsync();

            var orders = await query
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync(); 

            var mappedOrders = _mapper.Map<List<OrderListDtoAdminPanel>>(orders);

            return new PaginatedResultDto<OrderListDtoAdminPanel>
            {
                Items = mappedOrders,
                TotalCount = totalCount,
                PageNumber = pageNumber,
                PageSize = pageSize
            };
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

                string statusText = updateDto.NewStatus.ToString();
                string shortOrderId = order.Id.ToString().Substring(0, 8).ToUpper();

                var notificationDto = new CreateNotificationDto
                {
                    AppUserId = order.AppUserId,
                    Title = "Order Status Updated",
                    Message = $"Your order #{shortOrderId} status has been successfully updated to '{statusText}'.",
                    NotificationType = "Order",
                    RelatedUrl = $"/Order/Index"
                };

                await _notificationService.TAddAsync(notificationDto);

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