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
                .ForMember(dest => dest.ProductName, opt => opt.MapFrom(src => src.Product!.Name))
                .ForMember(dest => dest.Price, opt => opt.MapFrom(src => src.Product!.Price))
                .ForMember(dest => dest.ProductImageUrl, opt => opt.MapFrom(src =>
                    (src.Product == null || src.Product.ProductImages == null || !src.Product.ProductImages.Any())
                    ? "/img/no-image.png"
                    : (
                        (src.Product.ProductImages.FirstOrDefault(x => x.IsMain) ?? src.Product.ProductImages.First()).ImageUrl != null &&
                        (src.Product.ProductImages.FirstOrDefault(x => x.IsMain) ?? src.Product.ProductImages.First()).ImageUrl != ""
                        ? (src.Product.ProductImages.FirstOrDefault(x => x.IsMain) ?? src.Product.ProductImages.First()).ImageUrl
                        : "/img/no-image.png"
                      )));

            CreateMap<Cart, CartDto>()
                .ForMember(dest => dest.Items, opt => opt.MapFrom(src => src.CartItems));

            CreateMap<CreateCartItemDto, CartItem>()
                .ForMember(dest => dest.ProductId, opt => opt.MapFrom(src => src.ProductId))
                .ForMember(dest => dest.Quantity, opt => opt.MapFrom(src => src.Quantity));
        }
    }
}
