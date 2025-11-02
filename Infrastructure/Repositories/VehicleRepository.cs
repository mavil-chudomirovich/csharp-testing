using Application.AppExceptions;
using Application.Constants;
using Application.Dtos.Common.Request;
using Application.Dtos.Common.Response;
using Application.Dtos.VehicleModel.Respone;
using Application.Helpers;
using Application.Repositories;
using Domain.Entities;
using Infrastructure.ApplicationDbContext;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.EntityFrameworkCore;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;

namespace Infrastructure.Repositories
{
    public class VehicleRepository : GenericRepository<Vehicle>, IVehicleRepository
    {
        public VehicleRepository(IGreenWheelDbContext dbContext) : base(dbContext)
        {
        }

        public async Task<Vehicle?> GetByLicensePlateAsync(string licensePlate)
        {
            return await _dbContext.Vehicles
                .Include(v => v.Model)
                .FirstOrDefaultAsync(x => x.LicensePlate == licensePlate);
        }

       public async Task<PageResult<Vehicle>> GetAllAsync(PaginationParams pagination, string? name, Guid? stationId, int? status, string? licensePlate)
        {
            var vehicles = _dbContext.Vehicles
                            .Include(v => v.Model)
                            .OrderByDescending(x => x.CreatedAt)
                            .AsQueryable();
            if (!string.IsNullOrEmpty(name)) vehicles = vehicles.Where(v => v.Model.Name.ToLower().Contains(name.ToLower()));
            if (stationId != null) vehicles = vehicles.Where(v => v.StationId == stationId);
            if (status != null) vehicles = vehicles.Where(v => v.Status == status);
            if (!string.IsNullOrEmpty(licensePlate)) vehicles = vehicles.Where(v => v.LicensePlate.ToLower().Contains(licensePlate.ToLower()));

            var totalCount = await vehicles.CountAsync();

            var listItem = await vehicles.ApplyPagination(pagination)
                .ToListAsync();

            return new PageResult<Vehicle>(listItem, pagination.PageNumber, pagination.PageSize, totalCount);
        }

        public async Task<IEnumerable<Vehicle>?> GetVehicles (Guid stationId, Guid modelId)
        {
            // Query lọc trực tiếp từ DB (không ToList trước)
            var vehicles = await _dbContext.Vehicles
                .Include(v => v.Model)
                .Include(v => v.RentalContracts) //join bảng rentalContracts để lấy xe có hợp đồng
                .OrderByDescending(x => x.CreatedAt)
                .Where
                (
                    v => v.StationId == stationId
                        && v.ModelId == modelId
                        && v.Status != (int)VehicleStatus.Maintenance
                ).AsNoTracking().ToListAsync();

            foreach (var v in vehicles)
            {
                v.RentalContracts = v.RentalContracts.Where(rc => rc.Status != (int)RentalContractStatus.Cancelled &&
                                              rc.Status != (int)RentalContractStatus.Completed).ToList();
            }
            return vehicles;
        }

        public async Task<Vehicle?> GetByIdOptionAsync(Guid id, bool includeModel = false)
        {
            IQueryable<Vehicle> query = _dbContext.Vehicles.AsQueryable();
            if (includeModel)
            {
                query = query.Include(i => i.Model);
            }
            return await query.FirstOrDefaultAsync(i => i.Id == id);
        }

        public async Task<int> CountVehiclesInStationAsync(Guid[] vehicleIds, Guid stationId)
        {
            return await _dbContext.Vehicles.CountAsync(x => vehicleIds.Contains(x.Id) && x.StationId == stationId);
        }

        public async Task UpdateStationForDispatchAsync(Guid dispatchId, Guid toStationId)
        {
            var vehicleIds = await _dbContext.DispatchRequestVehicles
                .Where(x => x.DispatchRequestId == dispatchId)
                .Select(x => x.VehicleId)
                .ToListAsync();

            if (vehicleIds.Count == 0)
                return;

            var vehicles = await _dbContext.Vehicles
                .Where(v => vehicleIds.Contains(v.Id))
                .ToListAsync();

            foreach (var v in vehicles)
            {
                v.StationId = toStationId;
                v.UpdatedAt = DateTimeOffset.UtcNow;
            }

            await _dbContext.SaveChangesAsync();
        }

        public async Task<IEnumerable<Vehicle>> GetAllAsync(Guid? stationId, int? status)
        {
            var query = _dbContext.Vehicles
                .AsQueryable();
            if(stationId != null)
            {
                query = query.Where(v => v.StationId == stationId);
            }
            if(status != null)
            {
                query = query.Where(v => v.Status == status);
            }
            return await query.ToListAsync();
        }
        public async Task<int> CountAvailableVehiclesByModelAsync(Guid stationId, Guid modelId)
        {
            return await _dbContext.Vehicles
                .Where(v =>
                    v.StationId == stationId &&
                    v.ModelId == modelId &&
                    v.Status == (int)VehicleStatus.Available &&
                    v.DeletedAt == null)
                .CountAsync();
        }

        public async Task<List<Vehicle>> GetAvailableVehiclesByIdsAsync(Guid stationId, Guid[] vehicleIds)
        {
            if (vehicleIds == null || vehicleIds.Length == 0)
                return new List<Vehicle>();

            return await _dbContext.Vehicles
                .Where(v =>
                    vehicleIds.Contains(v.Id) &&
                    v.StationId == stationId &&
                    v.Status == (int)VehicleStatus.Available &&
                    v.DeletedAt == null)
                .ToListAsync();
        }
    }
}