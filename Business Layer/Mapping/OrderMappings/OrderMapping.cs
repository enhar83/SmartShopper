using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;
using Core_Layer.Dtos.CartDtos;
using Core_Layer.Dtos.OrderDtos;
using Entity_Layer;

namespace Business_Layer.Mapping.OrderMappings
{
    public class OrderMapping:Profile
    {
        public OrderMapping()
        {
            CreateMap<CartItemDto, OrderItem>()
                .ForMember(dest => dest.PriceAtPurchase, opt => opt.MapFrom(src => src.Price));

            CreateMap<Order, OrderListDto>()
                .ForMember(dest => dest.OrderItems, opt => opt.MapFrom(src => src.OrderItems));

            CreateMap<OrderItem, OrderItemListDto>()
                .ForMember(dest => dest.ProductName, opt => opt.MapFrom(src => src.Product.Name))
                .ForMember(dest => dest.ProductImageUrl, opt => opt.MapFrom(src =>
                    src.Product.ProductImages != null && src.Product.ProductImages.Any()
                    ? src.Product.ProductImages.FirstOrDefault()!.ImageUrl
                    : "/images/no-image.png"));
        }
    }
}
