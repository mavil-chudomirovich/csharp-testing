using Application.Constants;
using Application.Dtos.Common.Request;
using Application.Dtos.Common.Response;
using Domain.Entities;
using Microsoft.AspNetCore.Identity.UI.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Repositories
{
    public interface IVehicleRepository : IGenericRepository<Vehicle>
    {
        Task<Vehicle?> GetByLicensePlateAsync(string licensePlate);

        Task<IEnumerable<Vehicle>?> GetVehicles(Guid stationId, Guid modelId);
        Task<Vehicle?> GetByIdOptionAsync(Guid id, bool includeModel = false);

        Task<int> CountVehiclesInStationAsync(Guid[] vehicleIds, Guid stationId);
        Task<PageResult<Vehicle>> GetAllAsync(PaginationParams pagination, string? name, Guid? stationId, int? status, string? licensePlate);
        Task UpdateStationForDispatchAsync(Guid dispatchId, Guid toStationId);
        Task<IEnumerable<Vehicle>> GetAllAsync(Guid? stationId, int? status);
        Task<int> CountAvailableVehiclesByModelAsync(Guid stationId, Guid modelId);
        Task<List<Vehicle>> GetAvailableVehiclesByIdsAsync(Guid stationId, Guid[] vehicleIds);
    }
}