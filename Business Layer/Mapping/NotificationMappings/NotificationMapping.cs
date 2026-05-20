using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;
using Core_Layer.Dtos.NotificationDtos;
using Entity_Layer;

namespace Business_Layer.Mapping.NotificationMappings
{
    public class NotificationMapping:Profile
    {
        public NotificationMapping()
        {
            CreateMap<CreateNotificationDto, Notification>().ReverseMap();
        }
    }
}
