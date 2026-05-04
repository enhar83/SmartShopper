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
    public class ProductMapping : Profile
    {
        public ProductMapping()
        {
            CreateMap<AddProductDto, Product>().ReverseMap();

            CreateMap<Product, ProductListDtoAdminPanel>()
                .ForMember(dest => dest.SubCategoryName, opt => opt.MapFrom(src =>
                    src.SubCategory != null ? src.SubCategory.Name : string.Empty))
                .ForMember(dest => dest.CategoryName, opt => opt.MapFrom(src =>
                    (src.SubCategory != null && src.SubCategory.Category != null)
                    ? src.SubCategory.Category.Name
                    : string.Empty));

            CreateMap<UpdateProductDto, Product>().ReverseMap();

            CreateMap<Product, ProductListDto>()
                .ForMember(dest => dest.CategoryName, opt => opt.MapFrom(src => src.SubCategory != null ? src.SubCategory.Category.Name : null))
                .ForMember(dest => dest.SubCategoryName, opt => opt.MapFrom(src => src.SubCategory != null ? src.SubCategory.Name : null))
                .ForMember(dest => dest.MainImageUrl, opt => opt.MapFrom(src =>
                    src.ProductImages != null && src.ProductImages.Any(x => x.IsMain)
                    ? src.ProductImages.FirstOrDefault(x => x.IsMain)!.ImageUrl
                    : "NO_IMAGE"))
                .ReverseMap();

            CreateMap<Product, SimilarProductsForProductDetailDto>()
                .ForMember(dest => dest.CategoryName, opt => opt.MapFrom(src => src.SubCategory != null ? src.SubCategory.Category.Name : null))
                .ForMember(dest => dest.SubCategoryName, opt => opt.MapFrom(src => src.SubCategory != null ? src.SubCategory.Name : null))
                .ForMember(dest => dest.MainImageUrl, opt => opt.MapFrom(src =>
                    src.ProductImages != null && src.ProductImages.Any(x => x.IsMain)
                    ? src.ProductImages.FirstOrDefault(x => x.IsMain)!.ImageUrl
                    : "NO_IMAGE"))
                .ReverseMap();

            CreateMap<Product, ProductDetailDto>()
                .ForMember(dest => dest.CategoryName, opt => opt.MapFrom(src => src.SubCategory != null ? src.SubCategory.Category.Name : null))
                .ForMember(dest => dest.SubCategoryName, opt => opt.MapFrom(src => src.SubCategory != null ? src.SubCategory.Name : null))
                .ForMember(dest => dest.MainImageUrl, opt => opt.MapFrom(src =>
                    src.ProductImages != null && src.ProductImages.Any(x => x.IsMain)
                    ? src.ProductImages.FirstOrDefault(x => x.IsMain)!.ImageUrl
                    : "NO_IMAGE"))
                .ReverseMap();
        }
    }
}
