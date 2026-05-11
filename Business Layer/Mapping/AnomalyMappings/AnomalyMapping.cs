using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;
using Core_Layer.Dtos.OrderAnomalyDtos;
using Entity_Layer.Common;

namespace Business_Layer.Mapping.AnomalyMappings
{
    public class AnomalyMapping:Profile
    {
        public AnomalyMapping()
        {
            CreateMap<OrderAnomalyResult, OrderAnomalyDto>()
                .ForMember(dest => dest.OrderTotal, opt => opt.MapFrom(src => src.Order.TotalPrice))
                .ForMember(dest => dest.OrderDate, opt => opt.MapFrom(src => src.Order.CreatedDate))
                .ForMember(dest => dest.CustomerName, opt => opt.MapFrom(src => src.Order.AppUser.Name + " " + src.Order.AppUser.Surname));
        }
    }
}
