using Application.Abstractions;
using Application.AppExceptions;
using Application.Constants;
using Application.Dtos.Common.Request;
using Application.Dtos.Common.Response;
using Application.Dtos.Vehicle.Request;
using Application.Dtos.Vehicle.Respone;
using Application.Repositories;
using AutoMapper;
using Domain.Entities;

namespace Application
{
    public class VehicleService : IVehicleService
    {
        private readonly IVehicleRepository _vehicleRepository;
        private readonly IMapper _mapper;

        public VehicleService(IVehicleRepository vehicleRepository, IMapper mapper)
        {
            _vehicleRepository = vehicleRepository;
            _mapper = mapper;
        }

        public async Task<Guid> CreateVehicleAsync(CreateVehicleReq createVehicleReq)
        {
            if (await _vehicleRepository.GetByLicensePlateAsync(createVehicleReq.LicensePlate) != null)
            {
                throw new ConflictDuplicateException(Message.VehicleMessage.LicensePlateIsExist);
            }
            var vehicle = _mapper.Map<Vehicle>(createVehicleReq);
            Guid id;
            do
            {
                id = new Guid();
            } while (await _vehicleRepository.GetByIdAsync(id) != null);
            vehicle.Status = (int)VehicleStatus.Available;
            vehicle.CreatedAt = vehicle.UpdatedAt = DateTimeOffset.UtcNow;
            vehicle.DeletedAt = null;
            return await _vehicleRepository.AddAsync(vehicle);
        }

        public async Task DeleteVehicle(Guid id)
        {
            await _vehicleRepository.DeleteAsync(id);
        }

        public async Task<PageResult<VehicleViewRes>> GetAllAsync(PaginationParams pagination, string? name, Guid? stationId, int? status, string? licensePlate)
        {
            var vehicles = await _vehicleRepository.GetAllAsync(pagination, name, stationId, status, licensePlate);

            var mappedItems = _mapper.Map<IEnumerable<VehicleViewRes>>(vehicles.Items);

            return new PageResult<VehicleViewRes>(
                mappedItems,
                vehicles.PageNumber,
                vehicles.PageSize,
                vehicles.Total
            );
        }

        public async Task<IEnumerable<Vehicle>> GetAllAsync(Guid? stationId, int? status)
        {
            return await _vehicleRepository.GetAllAsync(stationId, status) ?? [];
        }

        public async Task<VehicleViewRes> GetVehicleById(Guid id)
        {
            var vehicle = await _vehicleRepository.GetByIdOptionAsync(id, includeModel: true);
            if (vehicle == null) throw new NotFoundException(Message.VehicleMessage.NotFound);
            return _mapper.Map<VehicleViewRes>(vehicle);
        }

        //public async Task<Vehicle> GetVehicle(Guid stationId, Guid modelId,
        //    DateTimeOffset startDate, DateTimeOffset endDate)
        //{
        //    return await _vehicleRepository.GetVehicle(stationId, modelId, startDate, endDate);
        //}

        public async Task<int> UpdateVehicleAsync(Guid Id, UpdateVehicleReq updateVehicleReq)
        {
            var vehicleFromDb = await _vehicleRepository.GetByIdAsync(Id);
            if (vehicleFromDb == null)
            {
                throw new NotFoundException(Message.VehicleMessage.NotFound);
            }
            if (updateVehicleReq.LicensePlate != null)
            {
                vehicleFromDb.LicensePlate = updateVehicleReq.LicensePlate;
            }
            if (updateVehicleReq.Status != null)
            {
                vehicleFromDb.Status = (int)updateVehicleReq.Status;
            }
            if (updateVehicleReq.StationId != null)
            {
                vehicleFromDb.StationId = (Guid)updateVehicleReq.StationId;
            }
            if (updateVehicleReq.ModelId != null)
            {
                vehicleFromDb.ModelId = (Guid)updateVehicleReq.ModelId;
            }
            return await _vehicleRepository.UpdateAsync(vehicleFromDb);
        }
    }
}