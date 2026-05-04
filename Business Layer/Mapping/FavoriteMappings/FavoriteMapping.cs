using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;
using Core_Layer.Dtos.FavoriteDtos;
using Entity_Layer;

namespace Business_Layer.Mapping.FavoriteMappings
{
    public class FavoriteMapping:Profile
    {
        public FavoriteMapping() 
        {
            CreateMap<Favorite, WishlistDto>()
                .ForMember(dest => dest.FavoriteId, opt => opt.MapFrom(src => src.Id))
                .ForMember(dest => dest.ProductName, opt => opt.MapFrom(src => src.Product!.Name))
                .ForMember(dest => dest.Price, opt => opt.MapFrom(src => src.Product!.Price))
                .ForMember(dest => dest.CategoryName, opt => opt.MapFrom(src =>
                    src.Product!.SubCategory!.Category!.Name))
                .ForMember(dest => dest.SubCategoryName, opt => opt.MapFrom(src =>
                    src.Product!.SubCategory!.Name))
                .ForMember(dest => dest.ImageUrl, opt => opt.MapFrom(src =>
                    src.Product!.ProductImages!.FirstOrDefault() != null
                        ? src.Product!.ProductImages!.First().ImageUrl
                        : "/img/no-image.png"));
        }
    }
}
