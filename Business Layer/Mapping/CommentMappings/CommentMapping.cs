using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;
using Core_Layer.Dtos.CommentDtos;
using Entity_Layer;

namespace Business_Layer.Mapping.CommentMappings
{
    public class CommentMapping : Profile
    {
        public CommentMapping()
        {
            CreateMap<CreateCommentDto, Comment>() 
                .ForMember(dest => dest.IsApproved, opt => opt.MapFrom(src => false))
                .ForMember(dest => dest.AppUserId, opt => opt.Ignore());

            CreateMap<Comment, ResultCommentDto>()
                .ForMember(dest => dest.UserName, opt => opt.MapFrom(src => src.AppUser.Name))
                .ForMember(dest => dest.UserSurname, opt => opt.MapFrom(src => src.AppUser.Surname));

            CreateMap<Comment, CommentListAdminPanelDto>()
                .ForMember(dest => dest.ProductName, opt => opt.MapFrom(src => src.Product.Name))
                .ForMember(dest => dest.CustomerFullName, opt => opt.MapFrom(src => src.AppUser.Name + " " + src.AppUser.Surname))
                .ForMember(dest => dest.CustomerEmail, opt => opt.MapFrom(src => src.AppUser.Email));

            CreateMap<Comment, UserCommentListDto>()
                .ForMember(dest => dest.ProductName, opt => opt.MapFrom(src => src.Product.Name));
        }
    }
}
