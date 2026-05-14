using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;
using Core_Layer.Dtos.DiscountDtos;
using Entity_Layer;

namespace Business_Layer.Mapping.DiscountMappings
{
    public class DiscountProfile : Profile
    {
        public DiscountProfile()
        {
            CreateMap<DiscountCreateDto, Discount>()
                .ForMember(dest => dest.IsDeleted, opt => opt.MapFrom(src => false));

            CreateMap<Discount, DiscountListDto>();

            CreateMap<DiscountUpdateDto, Discount>();

            CreateMap<Discount, DiscountUpdateDto>();

            CreateMap<AssignDiscountDto, DiscountCustomer>()
                .ForMember(dest => dest.IsUsed, opt => opt.MapFrom(src => false))
                .ForMember(dest => dest.IsDeleted, opt => opt.MapFrom(src => false));

            CreateMap<DiscountCustomer, DiscountAssignedUserDto>()
                .ForMember(dest => dest.AssignmentId, opt => opt.MapFrom(src => src.Id))
                .ForMember(dest => dest.UserFullName, opt => opt.MapFrom(src => src.AppUser.Name + " " + src.AppUser.Surname))
                .ForMember(dest => dest.Email, opt => opt.MapFrom(src => src.AppUser.Email))
                .ForMember(dest => dest.AssignedDate, opt => opt.MapFrom(src => src.CreatedDate));
        }
    }
}
