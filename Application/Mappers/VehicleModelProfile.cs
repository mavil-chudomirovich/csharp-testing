using Application.Dtos.Vehicle.Respone;
using Application.Dtos.VehicleModel.Request;
using Application.Dtos.VehicleModel.Respone;
using AutoMapper;
using Domain.Entities;

namespace Application.Mappers
{
    public class VehicleModelProfile : Profile
    {
        public VehicleModelProfile()
        {
            CreateMap<CreateVehicleModelReq, VehicleModel>();
            //CreateMap<VehicleModel, VehicleModelViewRes>();

            //CreateMap<VehicleModel, VehicleModelViewRes>()
            //    .ForMember(dest => dest.ImageUrls, opt => opt.MapFrom(src => src.ModelImages.Select(mi => mi.Url)))
            //    .ForMember(dest => dest.AvailableVehicleCount, opt => opt.MapFrom(src => src.Vehicles.Count()));
            CreateMap<VehicleModel, VehicleModelViewRes>()
               .ForMember(dest => dest.Brand, opt => opt.MapFrom(src => src.Brand))
               .ForMember(dest => dest.Segment, opt => opt.MapFrom(src => src.Segment))
               .ForMember(dest => dest.ImageUrls, opt => opt.MapFrom(src => src.ModelImages.Select(mi => mi.Url)))
               .ForMember(dest => dest.AvailableVehicleCount, opt => opt.MapFrom(src => src.Vehicles.Count()));
            CreateMap<VehicleModel, VehicleModelRes>();
            
            CreateMap<VehicleModel, VehicleModelImagesRes>()
                .ForMember(dest => dest.ImageUrls, opt => opt.MapFrom(src => src.ModelImages.Select(mi => mi.Url)));
            CreateMap<UpdateVehicleModelReq, VehicleModel>()
                .ForAllMembers(opts => opts.Condition((src, dest, srcMember) => srcMember != null));
            CreateMap<VehicleModel, VehicleModelMainImageRes>();
        }
    }
}