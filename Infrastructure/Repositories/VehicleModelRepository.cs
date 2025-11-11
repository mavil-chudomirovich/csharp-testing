using Application.Constants;
using Application.Dtos.VehicleModel.Respone;
using Application.Repositories;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using Domain.Entities;
using Infrastructure.ApplicationDbContext;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;

namespace Infrastructure.Repositories
{
    public class VehicleModelRepository : GenericRepository<VehicleModel>, IVehicleModelRepository
    {
        private readonly IMapper _mapper;
        private readonly IMemoryCache _cache;

        public VehicleModelRepository(IGreenWheelDbContext dbContext, IMapper mapper, IMemoryCache cache) : base(dbContext)
        {
            _mapper = mapper;
            _cache = cache;
        }
        public async Task<IEnumerable<VehicleModel>> GetAllAsync(string? name, Guid? segmentId)
        {
            var models = _dbContext.VehicleModels
                            .Include(vm => vm.Brand)
                            .Include(vm => vm.Segment)
                            .Include(vm => vm.ModelImages)
                            .Include(vm => vm.Vehicles)
                            .OrderByDescending(x => x.CreatedAt)
                            .AsQueryable();
            if (!string.IsNullOrEmpty(name)) models = models.Where(vm => vm.Name.ToLower().Contains(name.ToLower()));
            if (segmentId != null) models = models.Where(vm => vm.SegmentId == segmentId);
            return await models.ToListAsync();
        }

        public async Task<IEnumerable<VehicleModel>> FilterVehicleModelsAsync(
            Guid stationId,
            DateTimeOffset startDate,
            DateTimeOffset endDate,
            Guid? segmentId = null)
        {
            if ((endDate - startDate).TotalHours < 24)
                throw new ArgumentException(Message.VehicleModelMessage.RentTimeIsNotAvailable);
            var businessVariables = _cache!.Get<List<BusinessVariable>>(Common.SystemCache.BusinessVariables);
            var bufferDay = businessVariables!.FirstOrDefault(b => b.Key == (int)BusinessVariableKey.RentalContractBufferDay)?.Value;
            var startBuffer = startDate.AddDays(-(int)bufferDay!);
            var endBuffer = endDate.AddDays((int)bufferDay!);

            // Query cơ bản
            var query = _dbContext.VehicleModels
                .Include(vm => vm.Vehicles)
                    .ThenInclude(v => v.RentalContracts)
                .Include(vm => vm.Vehicles)
                    .ThenInclude(v => v.Station)
                .Include(vm => vm.Brand)
                .Include(vm => vm.Segment)
                .OrderByDescending(vm => vm.Vehicles.Min(v => v.Status))
                .AsNoTracking()
                .AsQueryable();
            if (segmentId != null)
                query = query.Where(vm => vm.SegmentId == segmentId.Value);
            var models = await query.ToListAsync();
            foreach (var model in models)
            {
                var vehicles = model.Vehicles.Where(v => CheckAvailableVehicle(v, stationId, startBuffer, endBuffer));
                model.Vehicles = vehicles.ToList();

            }
            return models;
        }


        public async Task<VehicleModel?> GetByIdAsync(
            Guid id,
            Guid stationId,
            DateTimeOffset startDate,
            DateTimeOffset endDate)
        {
            var businessVariables = _cache!.Get<List<BusinessVariable>>(Common.SystemCache.BusinessVariables);
            var bufferDay = businessVariables!.FirstOrDefault(b => b.Key == (int)BusinessVariableKey.RentalContractBufferDay)?.Value;
            var startBuffer = startDate.AddDays(-(int)bufferDay!);
            var endBuffer = endDate.AddDays((int)bufferDay!);

            // Load model + ảnh + brand + segment
            var model = await _dbContext.VehicleModels
                .Include(vm => vm.ModelImages)
                .Include(vm => vm.Vehicles)
                    .ThenInclude(v => v.RentalContracts)
                .Include(vm => vm.Brand)
                .Include(vm => vm.Segment)
                .OrderByDescending(vm => vm.Vehicles.Min(v => v.Status))
                .AsNoTracking()
                .FirstOrDefaultAsync(vm => vm.Id == id && vm.DeletedAt == null);

            if (model == null) return null;

            var vehicles = model.Vehicles.Where(v => CheckAvailableVehicle(v, stationId, startBuffer, endBuffer));
            model.Vehicles = vehicles.ToList();

            return model;
        }
        private bool CheckAvailableVehicle(Vehicle vehicle, Guid stationId, DateTimeOffset startBuffer, DateTimeOffset endBuffer)
        {
            //return vehicle.StationId == stationId
            //        &&
            //        (vehicle.Status == (int)VehicleStatus.Available
            //        ||
            //        ((vehicle.Status == (int)VehicleStatus.Unavaible || vehicle.Status == (int)VehicleStatus.Rented)) &&
            //                                        vehicle.RentalContracts.Any(rc => rc.Status == (int)RentalContractStatus.Active) &&
            //                                        !vehicle.RentalContracts.Any(rc =>
            //                                            rc.Status == (int)RentalContractStatus.Active &&
            //                                            startBuffer <= rc.EndDate &&
            //                                            endBuffer >= rc.StartDate
            //
            //));
            if (vehicle.Status != (int)VehicleStatus.Available
               && vehicle.Status != (int)VehicleStatus.Unavaible
               && vehicle.Status != (int)VehicleStatus.Rented)
                return false;

            if (vehicle.StationId != stationId)
                return false;

            if (vehicle.Status == (int)VehicleStatus.Available)
                return true;

            var flagContracts = vehicle.RentalContracts
                .Where(rc => rc.Status == (int)RentalContractStatus.Active
                          || rc.Status == (int)RentalContractStatus.Returned
                          || rc.Status == (int)RentalContractStatus.UnavailableVehicle)
                .ToList();

            if (!flagContracts.Any())
                return true;

            bool overlap = flagContracts.Any(rc =>
                startBuffer <= rc.EndDate && endBuffer >= rc.StartDate
            );
            return !overlap;
        }

        public async Task<VehicleModel?> GetWithoutSearchAsync(Guid id)
        {
            var model = await _dbContext.VehicleModels
               .Include(vm => vm.ModelImages)
               .Include(vm => vm.Vehicles)
                   .ThenInclude(v => v.RentalContracts)
               .Include(vm => vm.Brand)
               .Include(vm => vm.Segment)
               .OrderByDescending(vm => vm.Vehicles.Min(v => v.Status))
               .AsNoTracking()
               .FirstOrDefaultAsync(vm => vm.Id == id);
            return model;
        }
    }
}