using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;
using Core_Layer.Dtos.UserAddressDtos;
using Entity_Layer;

namespace Business_Layer.Mapping.UserAddressMappings
{
    public class UserAddressMapping: Profile
    {
        public UserAddressMapping() 
        {
            CreateMap<UserAddress, UserAddressListDto>().ReverseMap();
        }
    }
}
