using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;
using Core_Layer.Dtos.ProductImagesDtos;
using Entity_Layer;

namespace Business_Layer.Mapping.ProductImageMappings
{
    public class ProductImageMappings:Profile
    {
        public ProductImageMappings()
        {
            CreateMap<AddProductImageDto, ProductImage>()
                .ForMember(dest => dest.ImageUrl, opt => opt.Ignore()) 
                .ReverseMap();

            CreateMap<ProductImage, ProductImageListDto>().ReverseMap();
        }
    }
}
