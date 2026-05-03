using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;
using Core_Layer.Dtos.ProfileDtos;
using Entity_Layer;

namespace Business_Layer.Mapping.ProfileMappings
{
    public class ProfileMapping:Profile
    {
        public ProfileMapping() 
        {
            CreateMap<ViewProfileDto, AppUser>().ReverseMap();
        }   
    }
}
