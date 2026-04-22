using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;
using Business_Layer.Validators.SubCategoryValidators;
using Core_Layer.Dtos.CategoryDtos;
using Core_Layer.Dtos.SubCategoryDtos;
using Entity_Layer;

namespace Business_Layer.Mapping.SubCategoryMapping
{
    public class SubCategoryMapping:Profile
    {
        public SubCategoryMapping()
        {
            CreateMap<AddSubCategoryDto,SubCategory>().ReverseMap();
            CreateMap<SubCategory, SubCategoryListDto>().ReverseMap();
            CreateMap<UpdateSubCategoryDto, SubCategory>().ReverseMap();
        }
    }
}
