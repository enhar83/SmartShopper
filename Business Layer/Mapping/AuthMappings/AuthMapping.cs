using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;
using Core_Layer.Dtos.AuthDtos;
using Core_Layer.Dtos.CategoryDtos;
using Entity_Layer;

namespace Business_Layer.Mapping.AuthMappings
{
    public class AuthMapping:Profile
    {
        public AuthMapping() 
        {
            CreateMap<RegisterDto, AppUser>().ReverseMap();
        }
    }
}
