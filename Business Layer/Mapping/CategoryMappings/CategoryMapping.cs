using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;
using Core_Layer.Dtos.CategoryDtos;
using Entity_Layer;

namespace Business_Layer.Mapping.CategoryMappings
{
    public class CategoryMapping:Profile
    {
        public CategoryMapping() 
        {
            CreateMap<Category, CategoryListDto>().ReverseMap();
            CreateMap<AddCategoryDto, Category>().ReverseMap();
            CreateMap<UpdateCategoryDto, Category>().ReverseMap();

            CreateMap<Category, CategoryListInSidebarDto>()
                .ForMember(dest => dest.CategoryName, opt => opt.MapFrom(src => src.Name))
                .ForMember(dest => dest.SubCategories, opt => opt.MapFrom(src => src.SubCategories.Where(s => !s.IsDeleted)))
                .ReverseMap();
        }
    }
}
