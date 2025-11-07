using Application.Constants;
using Application.Dtos.Dispatch.Request;
using Application.Dtos.Dispatch.Response;
using Application.Helpers;
using AutoMapper;
using Domain.Entities;

namespace Application.Mappers
{
    public class DispatchProfile : Profile
    {
        public DispatchProfile()
        {
            CreateMap<CreateDispatchReq, DispatchRequest>()
                .ForMember(dest => dest.Status, opt => opt.MapFrom(_ => (int)DispatchRequestStatus.Pending))
                .ForAllOtherMembers(opt => opt.Ignore());

            // Entity -> Res
            CreateMap<DispatchRequest, DispatchRes>()
                .ForMember(dest => dest.FromStationName,
                    opt => opt.MapFrom(src => src.FromStation != null
                        ? src.FromStation.Name : null))
                .ForMember(dest => dest.ToStationName, opt => opt.MapFrom(src => src.ToStation.Name))
                .ForMember(dest => dest.Status, opt => opt.MapFrom(src => (DispatchRequestStatus)src.Status))
                .ForMember(dest => dest.RequestAdminName,
                    opt => opt.MapFrom(src => src.RequestAdmin.User.FirstName + " " + src.RequestAdmin.User.LastName))
                .ForMember(dest => dest.ApprovedAdminName,
                    opt => opt.MapFrom(src => src.ApprovedAdmin != null
                        ? src.ApprovedAdmin.User.FirstName + " " + src.ApprovedAdmin.User.LastName
                        : null))
                .ForMember(dest => dest.Description,
                    opt => opt.MapFrom(src => JsonHelper.DeserializeJSON<DispatchDescriptionDto>(src.Description)))
                .ForMember(dest => dest.FinalDescription,
                    opt => opt.MapFrom(src => JsonHelper.DeserializeJSON<DispatchDescriptionDto>(src.FinalDescription)));

            // Staffs + Vehicles
            CreateMap<DispatchRequestStaff, DispatchRequestStaffRes>();
            CreateMap<DispatchRequestVehicle, DispatchRequestVehicleRes>();
        }
    }
}