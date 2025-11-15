using Application.Dtos.CitizenIdentity.Response;
using Application.Dtos.DriverLicense.Response;
using Application.Dtos.Statistic.Responses;
using Application.Dtos.User.Request;
using Application.Dtos.User.Respone;
using Application.Helpers;
using AutoMapper;
using Domain.Entities;
using System;

namespace Application.Mappers
{
    public class UserProfile : Profile
    {
        public UserProfile()
        {
            CreateMap<User, UserProfileViewRes>()
                .ForMember(dest => dest.LicenseUrl,
                    opt => opt.MapFrom(src => src.DriverLicense != null ? src.DriverLicense.FrontImageUrl : null))
                .ForMember(dest => dest.CitizenUrl,
                    opt => opt.MapFrom(src => src.CitizenIdentity != null ? src.CitizenIdentity.FrontImageUrl : null))
                .ForMember(dest => dest.Station,
                    opt => opt.MapFrom(src => src.Staff != null ? src.Staff.Station : null))
                .ForMember(dest => dest.NeedSetPassword,
                    opt => opt.MapFrom(src => src.Password == null));

            CreateMap<UserRegisterReq, User>()
                .ForMember(dest => dest.Password,
                           opt => opt.MapFrom(src => PasswordHelper.HashPassword(src.Password)));

            CreateMap<CreateUserReq, User>();

            CreateMap<DriverLicense, DriverLicenseRes>()
            .ForMember(dest => dest.Class, opt => opt.MapFrom(src => src.Class.ToString()))
            .ForMember(dest => dest.Sex, opt => opt.MapFrom(src => src.Sex.ToString()));

            CreateMap<CitizenIdentity, CitizenIdentityRes>()
                .ForMember(dest => dest.Sex, opt => opt.MapFrom(src => src.Sex.ToString()));

            CreateMap<Staff, UserProfileViewRes>()
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.User.Id))
                .ForMember(dest => dest.Email, opt => opt.MapFrom(src => src.User.Email))
                .ForMember(dest => dest.FirstName, opt => opt.MapFrom(src => src.User.FirstName))
                .ForMember(dest => dest.LastName, opt => opt.MapFrom(src => src.User.LastName))
                .ForMember(dest => dest.Sex, opt => opt.MapFrom(src => src.User.Sex))
                .ForMember(dest => dest.DateOfBirth, opt => opt.MapFrom(src => src.User.DateOfBirth))
                .ForMember(dest => dest.AvatarUrl, opt => opt.MapFrom(src => src.User.AvatarUrl))
                .ForMember(dest => dest.Phone, opt => opt.MapFrom(src => src.User.Phone))
                .ForMember(dest => dest.LicenseUrl, opt => opt.MapFrom(src =>
                    src.User.DriverLicense != null
                        ? src.User.DriverLicense.FrontImageUrl
                        : null))
                .ForMember(dest => dest.CitizenUrl, opt => opt.MapFrom(src =>
                    src.User.CitizenIdentity != null
                        ? src.User.CitizenIdentity.FrontImageUrl
                        : null));
        }
    }
}