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
        }
    }
}
