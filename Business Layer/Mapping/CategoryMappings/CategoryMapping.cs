using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;
using Core_Layer.Dtos;
using Entity_Layer;

namespace Business_Layer.Mapping.CategoryMappings
{
    public class CategoryMapping:Profile
    {
        public CategoryMapping() 
        {
            CreateMap<AddCategoryDto, Category>().ReverseMap();
        }
    }
}
