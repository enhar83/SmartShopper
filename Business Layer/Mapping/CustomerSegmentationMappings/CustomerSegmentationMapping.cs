using AutoMapper;
using Core_Layer.Dtos.CustomerSegmentationDtos;
using Entity_Layer;

namespace SmartShopper.Business.Mappings
{
    public class CustomerSegmentationProfile : Profile
    {
        public CustomerSegmentationProfile()
        {
            CreateMap<CustomerSegmentationResult, CustomerSegmentResultDto>()
                .ForMember(dest => dest.UserFullName,
                           opt => opt.MapFrom(src => $"{src.AppUser.Name} {src.AppUser.Surname}"))
                .ForMember(dest => dest.Email,
                           opt => opt.MapFrom(src => src.AppUser.Email))
                .ForMember(dest => dest.StatusColor,
                           opt => opt.MapFrom(src => AssignColor(src.SegmentLabel)));

            CreateMap<CustomerSegmentationResult, CustomerSegmentDto>()
                .ForMember(dest => dest.UserFullName,
                           opt => opt.MapFrom(src => $"{src.AppUser.Name} {src.AppUser.Surname}"));
        }
        private string AssignColor(string label)
        {
            return label.ToLower() switch
            {
                "loyal" or "vip" => "#28a745", 
                "at-risk" => "#ffc107",       
                "churn" or "lost" => "#dc3545", 
                _ => "#6c757d"           
            };
        }
    }
}