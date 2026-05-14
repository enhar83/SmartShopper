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
        }
    }
}
