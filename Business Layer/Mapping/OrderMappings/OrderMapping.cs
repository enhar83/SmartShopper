using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;
using Core_Layer.Dtos.CartDtos;
using Entity_Layer;

namespace Business_Layer.Mapping.OrderMappings
{
    public class OrderMapping:Profile
    {
        public OrderMapping()
        {
            CreateMap<CartItemDto, OrderItem>()
                .ForMember(dest => dest.PriceAtPurchase, opt => opt.MapFrom(src => src.Price));
        }
    }
}
