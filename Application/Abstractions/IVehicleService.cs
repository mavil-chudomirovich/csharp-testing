using Application.Dtos.Common.Request;
using Application.Dtos.Common.Response;
using Application.Dtos.Vehicle.Request;
using Application.Dtos.Vehicle.Respone;
using Domain.Entities;

namespace Application.Abstractions
{
    public interface IVehicleService
    {
        Task<Guid> CreateVehicleAsync(CreateVehicleReq createVehicleReq);

        Task<int> UpdateVehicleAsync(Guid Id, UpdateVehicleReq updateVehicleReq);

        Task DeleteVehicle(Guid id);

        Task<PageResult<VehicleViewRes>> GetAllAsync(PaginationParams pagination, string? name, Guid? stationId, int? status, string? licensePlate);

        Task<VehicleViewRes> GetVehicleById(Guid id);

        Task<IEnumerable<Vehicle>> GetAllAsync(Guid? stationId, int? status);


        //Task<Vehicle> GetVehicle(Guid stationId, Guid modelId, DateTimeOffset startDate, DateTimeOffset endDate);
    }
}