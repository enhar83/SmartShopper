using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;
using Core_Layer.Dtos.CategoryDtos;
using Core_Layer.Dtos.ProductDtos;
using Entity_Layer;

namespace Business_Layer.Mapping.ProductMappings
{
    public class ProductMapping:Profile
    {
        public ProductMapping() 
        {
            CreateMap<AddProductDto, Product>().ReverseMap();

            CreateMap<Product, ProductListDto>()
                .ForMember(dest => dest.SubCategoryName, opt => opt.MapFrom(src =>
                    src.SubCategory != null ? src.SubCategory.Name : string.Empty))
                .ForMember(dest => dest.CategoryName, opt => opt.MapFrom(src =>
                    (src.SubCategory != null && src.SubCategory.Category != null)
                    ? src.SubCategory.Category.Name
                    : string.Empty));

            CreateMap<UpdateProductDto, Product>().ReverseMap();
        }
    }
}
