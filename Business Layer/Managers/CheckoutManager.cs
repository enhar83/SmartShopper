using AutoMapper;
using Core_Layer.Dtos.CartDtos;
using Core_Layer.Dtos.OrderDtos;
using Core_Layer.IRepositories;
using Core_Layer.IServices;
using Entity_Layer;
using Microsoft.EntityFrameworkCore; 
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Business_Layer.Managers
{
    public class CheckoutManager : ICheckoutService
    {
        private readonly IOrderRepository _orderRepository;
        private readonly IOrderItemRepository _orderItemRepository;
        private readonly IProductRepository _productRepository;
        private readonly IUserAddressRepository _addressRepository;
        private readonly ICartService _cartService;
        private readonly IDiscountCustomerRepository _discountCustomerRepository; 
        private readonly IUnitOfWork _uow;
        private readonly IMapper _mapper;

        public CheckoutManager(
            IOrderRepository orderRepository,
            IOrderItemRepository orderItemRepository,
            IProductRepository productRepository,
            IUserAddressRepository addressRepository,
            ICartService cartService,
            IDiscountCustomerRepository discountCustomerRepository,
            IUnitOfWork uow,
            IMapper mapper)
        {
            _orderRepository = orderRepository;
            _orderItemRepository = orderItemRepository;
            _productRepository = productRepository;
            _addressRepository = addressRepository;
            _cartService = cartService;
            _discountCustomerRepository = discountCustomerRepository;
            _uow = uow;
            _mapper = mapper;
        }

        public async Task<CheckoutSummaryDto> TGetCheckoutSummaryAsync(Guid userId)
        {
            var userCart = await _cartService.TGetUserCartAsync(userId);

            if (userCart == null || userCart.Items == null)
            {
                return new CheckoutSummaryDto
                {
                    Items = new List<CartItemDto>(),
                    SubTotal = 0,
                    TotalPrice = 0
                };
            }

            var summary = new CheckoutSummaryDto
            {
                Items = userCart.Items,
                SubTotal = userCart.GrandTotal
            };

            summary.TotalPrice = summary.SubTotal;
            return summary;
        }

        public async Task<bool> TPlaceOrderAsync(Guid userId, Guid addressId, Guid? discountId = null)
        {
            var userCart = await _cartService.TGetUserCartAsync(userId);
            if (userCart == null || !userCart.Items.Any())
                return false;

            var address = await _addressRepository.GetByIdAsync(addressId);
            if (address == null || address.AppUserId != userId)
                return false;

            try
            {
                string addressSnapshot = $"{address.Title}: {address.FullAddress} - {address.District}/{address.City} - {address.Country}";

                decimal subTotal = userCart.GrandTotal;
                decimal discountAmount = 0;
                decimal finalTotal = subTotal;
                Guid? appliedDiscountId = null;

                if (discountId.HasValue && discountId.Value != Guid.Empty)
                {
                    var assignment = await _discountCustomerRepository.GetAll()
                        .Include(x => x.Discount)
                        .FirstOrDefaultAsync(x => x.Id == discountId.Value && x.AppUserId == userId && !x.IsUsed && !x.IsDeleted);

                    if (assignment != null && assignment.Discount != null && !assignment.Discount.IsDeleted)
                    {
                        var discount = assignment.Discount;
                        var now = DateTime.Now;

                        bool isMinOrderValid = !discount.MinOrderAmount.HasValue || subTotal >= discount.MinOrderAmount.Value;
                        bool isDateValid = discount.StartDate <= now && discount.EndDate >= now;

                        if (isMinOrderValid && isDateValid)
                        {
                            if (discount.Type == Discount.DiscountType.Percentage)
                            {
                                discountAmount = (subTotal * discount.Value) / 100m;
                            }
                            else if (discount.Type == Discount.DiscountType.FixedAmount)
                            {
                                discountAmount = discount.Value;
                            }

                            finalTotal = subTotal - discountAmount;
                            if (finalTotal < 0) finalTotal = 0; 

                            appliedDiscountId = discount.Id; 
                        }
                    }
                }

                var order = new Order
                {
                    AppUserId = userId,
                    CreatedDate = DateTime.Now,
                    Status = OrderStatus.Pending,
                    AddressId = addressId,
                    DeliveryAddressSnapshot = addressSnapshot,
                    IsDeleted = false,

                    SubTotal = subTotal,
                    DiscountAmount = discountAmount > 0 ? discountAmount : null,
                    AppliedDiscountId = appliedDiscountId,
                    TotalPrice = finalTotal,

                    AppUser = null!,
                    UserAddress = null!
                };

                await _orderRepository.AddAsync(order);

                foreach (var item in userCart.Items)
                {
                    var product = await _productRepository.GetByIdAsync(item.ProductId);

                    if (product == null || product.Stock < item.Quantity)
                        return false;

                    var orderItem = new OrderItem
                    {
                        OrderId = order.Id,
                        ProductId = item.ProductId,
                        Quantity = item.Quantity,
                        PriceAtPurchase = item.Price,
                        CreatedDate = DateTime.Now,
                        IsDeleted = false,
                        Order = null!,
                        Product = null!
                    };

                    await _orderItemRepository.AddAsync(orderItem);

                    product.Stock -= item.Quantity;
                    _productRepository.Update(product);
                }

                await _cartService.TClearCartAsync(userId);
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