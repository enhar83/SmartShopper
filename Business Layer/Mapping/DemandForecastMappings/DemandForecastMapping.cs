using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;
using Core_Layer.Dtos.DemandForecastDtos;
using Entity_Layer;

namespace Business_Layer.Mapping.DemandForecastMappings
{
    public class DemandForecastMapping:Profile
    {
        public DemandForecastMapping()
        {
            CreateMap<RegionalDemandForecast, RegionalDemandForecastDto>();
        }
    }
}
