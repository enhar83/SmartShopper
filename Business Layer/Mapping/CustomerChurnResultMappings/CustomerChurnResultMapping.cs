using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;
using Business_Layer.MLModels.ChurnPredictionModels;
using Core_Layer.Dtos.ChurnPredictionDtos;
using Entity_Layer;

namespace Business_Layer.Mapping.CustomerChurnResultMappings
{
    public class CustomerChurnResultMapping:Profile
    {
        public CustomerChurnResultMapping()
        {
            CreateMap<CustomerChurnResult, ChurnPredictionResultDto>()
                .ForMember(dest => dest.UserFullName, opt => opt.MapFrom(src => $"{src.AppUser.Name} {src.AppUser.Surname}"))
                .ForMember(dest => dest.ChurnProbability, opt => opt.MapFrom(src => (double)src.ChurnProbability));
        }
    }
}
