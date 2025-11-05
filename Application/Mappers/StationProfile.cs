using Application.Dtos.Station.Request;
using Application.Dtos.Station.Respone;
using AutoMapper;
using Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Mappers
{
    public class StationProfile : Profile
    {
        public StationProfile()
        {
            CreateMap<Station, StationViewRes>();
            CreateMap<Station, StationSimpleRes>();
            CreateMap<StationCreateReq, Station>();
        }
    }
}
