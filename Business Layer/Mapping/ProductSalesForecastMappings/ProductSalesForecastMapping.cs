using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;
using Core_Layer.Dtos.ProductForecastDtos;
using Entity_Layer;

namespace Business_Layer.Mapping.ProductSalesForecastMappings
{
    public class ProductSalesForecastMapping:Profile
    {
        public ProductSalesForecastMapping() 
        {
            CreateMap<ProductSalesForecast, ProductSalesForecastDto>()
            .ForMember(dest => dest.ProductName, opt => opt.MapFrom(src => src.Product.Name))
            .ForMember(dest => dest.CurrentPrice, opt => opt.MapFrom(src => src.Product.Price));
        }
    }
}
