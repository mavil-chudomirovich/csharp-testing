using Application.Constants;
using Application.Dtos.CitizenIdentity.Request;
using Application.Dtos.CitizenIdentity.Response;
using Application.Dtos.Common.Request;
using Application.Dtos.Common.Response;
using Application.Dtos.DriverLicense.Request;
using Application.Dtos.DriverLicense.Response;
using Application.Dtos.Staff.Request;
using Application.Dtos.User.Request;
using Application.Dtos.User.Respone;
using Domain.Entities;
using Google.Apis.Auth;
using Microsoft.AspNetCore.Http;
using System.Security.Claims;

namespace Application.Abstractions
{
    public interface IUserService
    {
        Task<Guid> CreateAsync(CreateUserReq req);

        //Task<IEnumerable<UserProfileViewRes>> GetAllAsync(string? phone, string? citizenIdNumber, string? driverLicenseNumber);
        Task<PageResult<UserProfileViewRes>> GetAllWithPaginationAsync(
            PaginationParams pagination,
            string? phone,
            string? citizenIdNumber,
            string? driverLicenseNumber,
            string? roleName,
            Guid? stationId);

        Task<UserProfileViewRes?> GetByIdAsync(Guid id);

        Task<UserProfileViewRes> GetByPhoneAsync(string phone);

        Task<UserProfileViewRes> GetByCitizenIdentityAsync(string idNumber);

        Task<UserProfileViewRes> GetByDriverLicenseAsync(string number);

        Task<PageResult<UserProfileViewRes>> GetAllStaffAsync(PaginationParams pagination, string? name, Guid? stationId);

        Task DeleteCustomer(Guid id);
    }
}