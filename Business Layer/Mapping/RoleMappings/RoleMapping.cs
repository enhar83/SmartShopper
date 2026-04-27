using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;
using Core_Layer.Dtos.RoleDtos;
using Entity_Layer;

namespace Business_Layer.Mapping.RoleMappings
{
    public class RoleMapping: Profile
    {
        public RoleMapping()
        {
            CreateMap<CreateRoleDto, AppRole>()
                .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.RoleName))
                .ReverseMap();

            CreateMap<AppRole, RoleListDto>()
                .ForMember(dest => dest.RoleName, opt => opt.MapFrom(src => src.Name))
                .ReverseMap();

            CreateMap<UpdateRoleDto, AppRole>()
                .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.RoleName))
                .ReverseMap();
        }
    }
}
