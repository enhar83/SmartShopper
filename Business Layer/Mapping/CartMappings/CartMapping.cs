using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;
using Core_Layer.Dtos.CartDtos;
using Entity_Layer;

namespace Business_Layer.Mapping.CartMappings
{
    public class CartProfile : Profile
    {
        public CartProfile()
        {
            CreateMap<CartItem, CartItemDto>()
                .ForMember(dest => dest.ProductName, opt => opt.MapFrom(src => src.Product.Name))
                .ForMember(dest => dest.Price, opt => opt.MapFrom(src => src.Product.Price))
                .ForMember(dest => dest.ProductImageUrl, opt => opt.MapFrom(src =>
                    src.Product.ProductImages != null && src.Product.ProductImages.Any()
                    ? src.Product.ProductImages.First().ImageUrl
                    : null));

            CreateMap<Cart, CartDto>()
                .ForMember(dest => dest.Items, opt => opt.MapFrom(src => src.CartItems));
        }
    }
}
