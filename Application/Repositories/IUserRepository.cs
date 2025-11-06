using Application.Dtos.Common.Request;
using Application.Dtos.Common.Response;
using Domain.Entities;
using System;
using System.Collections;

namespace Application.Repositories
{
    public interface IUserRepository : IGenericRepository<User>
    {
        // added: Phương thức get role khi lấy user theo id (Phúc thêm)
        // Mục đích:  response /api/users/me trả về đầy đủ thông tin role,
        // giúp useAuth ở frontend biết chắc user có role “staff”.
        //Task<IEnumerable<User>> GetAllAsync(string? phone, string? citizenIdNumber, string? driverLicenseNumber);
        Task<PageResult<User>> GetAllWithPaginationAsync(
            PaginationParams pagination,
            string? phone,
            string? citizenIdNumber,
            string? driverLicenseNumber,
            string? roleName,
            Guid? stationId);

        Task<User?> GetByIdWithFullInfoAsync(Guid id);

        Task<User?> GetByEmailAsync(string email);

        Task<User?> GetByPhoneAsync(string phone);

        Task<PageResult<User>> GetAllStaffAsync(PaginationParams pagination, string? name, Guid? stationId);

        Task<IEnumerable<User?>> GetAllAsync(string role);
    }
}