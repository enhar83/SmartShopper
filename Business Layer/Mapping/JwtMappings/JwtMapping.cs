using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;
using Core_Layer.Dtos.JwtDtos;
using Entity_Layer;

namespace Business_Layer.Mapping.JwtMappings
{
    public class JwtMapping:Profile
    {
        public JwtMapping()
        {
            CreateMap<AppUser, JwtDto>()
                .ForMember(dest => dest.Token, opt => opt.Ignore());
        }       
    }
}
