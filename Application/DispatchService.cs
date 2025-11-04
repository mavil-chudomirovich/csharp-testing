using Application.Abstractions;
using Application.AppExceptions;
using Application.Constants;
using Application.Dtos.Dispatch.Request;
using Application.Dtos.Dispatch.Response;
using Application.Helpers;
using Application.Repositories;
using AutoMapper;
using Domain.Entities;
using System.Text;
using System.Text.Json;

namespace Application
{
    public class DispatchService : IDispatchRequestService
    {
        private readonly IDispatchRepository _repository;
        private readonly IMapper _mapper;
        private readonly IVehicleRepository _vehicleRepository;
        private readonly IStaffRepository _staffRepository;
        private readonly IVehicleModelRepository _vehicleModelRepository;

        public DispatchService(
            IDispatchRepository repository,
            IMapper mapper,
            IVehicleRepository vehicleRepository,
            IStaffRepository staffRepository,
            IVehicleModelRepository vehicleModelRepository)
        {
            _repository = repository;
            _mapper = mapper;
            _vehicleRepository = vehicleRepository;
            _staffRepository = staffRepository;
            _vehicleModelRepository = vehicleModelRepository;
        }

        // ================= CREATE =================
        public async Task<Guid> CreateAsync(Guid adminId, CreateDispatchReq req)
        {
            if (req is null)
                throw new BadRequestException(Message.DispatchMessage.InvalidStatus);

            var requestAdminStaff = await _staffRepository.GetByUserIdAsync(adminId)
                ?? throw new ForbidenException(Message.UserMessage.DoNotHavePermission);

            var fromStationId = req.FromStationId;
            var toStationId = requestAdminStaff.StationId;

            DispatchValidationHelper.EnsureDifferentStations(fromStationId, toStationId);

            if (req.NumberOfStaff is > 0)
            {
                var availableStaffCount =
                    await _staffRepository.CountAvailableStaffInStationAsync(fromStationId);
                if (req.NumberOfStaff > availableStaffCount)
                    throw new BadRequestException(Message.DispatchMessage.StaffNotEnoughtInFromStation);
            }

            if (req.Vehicles is { Length: > 0 })
            {
                foreach (var v in req.Vehicles)
                {
                    var availableVehicles = await _vehicleRepository
                        .CountAvailableVehiclesByModelAsync(fromStationId, v.ModelId);

                    if (availableVehicles < v.NumberOfVehicle)
                        throw new BadRequestException(Message.DispatchMessage.VehicleOrStaffNotInFromStation);
                }
            }
            var vehicleLines = new StringBuilder();

            if (req.Vehicles is { Length: > 0 })
            {
                foreach (var v in req.Vehicles)
                {
                    var model = await _vehicleModelRepository.GetByIdAsync(v.ModelId)
                        ?? throw new NotFoundException(Message.VehicleModelMessage.NotFound);
                    var modelName = model.Name;

                    vehicleLines.AppendLine($"      - Model: {modelName} (ID: {v.ModelId}) | Quantity: {v.NumberOfVehicle}");
                }
            }
            else
            {
                vehicleLines.AppendLine("      (No vehicle requested)");
            }

            var description = $@"
Requested Staff: {req.NumberOfStaff}
Requested Vehicles:
{vehicleLines}";
            var entity = new DispatchRequest
            {
                Id = Guid.NewGuid(),
                RequestAdminId = adminId,
                FromStationId = fromStationId,
                ToStationId = toStationId,
                Status = (int)DispatchRequestStatus.Pending,
                Description = description.Trim()
            };

            await _repository.AddAsync(entity);
            return entity.Id;
        }

        // ================= GET =================
        public async Task<IEnumerable<DispatchRes>> GetAllAsync(
            Guid? fromStationId,
            Guid? toStationId,
            DispatchRequestStatus? status)
        {
            var data = await _repository.GetAllExpandedAsync(
                fromStationId,
                toStationId,
                status.HasValue ? (int)status.Value : null);

            return _mapper.Map<IEnumerable<DispatchRes>>(data);
        }

        public async Task<DispatchRes?> GetByIdAsync(Guid id)
        {
            var entity = await _repository.GetByIdWithFullInfoAsync(id);
            return entity == null ? null : _mapper.Map<DispatchRes>(entity);
        }

        // ================= UPDATE STATUS =================
        public async Task UpdateAsync(
            Guid currentAdminId,
            Guid currentAdminStationId,
            Guid id,
            UpdateDispatchReq req)
        {
            var entity = await _repository.GetByIdAsync(id)
                ?? throw new NotFoundException(Message.DispatchMessage.NotFound);

            var currentStatus = (DispatchRequestStatus)entity.Status;
            var newStatus = (DispatchRequestStatus)req.Status;

            switch (newStatus)
            {
                case DispatchRequestStatus.Approved:
                    DispatchValidationHelper.EnsureCanUpdate(
                        currentAdminStationId,
                        entity.FromStationId,
                        currentStatus,
                        [DispatchRequestStatus.Pending],
                        Message.UserMessage.DoNotHavePermission,
                        Message.DispatchMessage.OnlyPendingCanApproveReject);

                    if (req.StaffIds == null || req.VehicleIds == null)
                        throw new BadRequestException(Message.DispatchMessage.IdNull);

                    await DispatchValidationHelper.ValidateStaffsInStationAsync(
                        _staffRepository, req.StaffIds, entity.FromStationId);
                    await DispatchValidationHelper.ValidateVehiclesInStationAsync(
                        _vehicleRepository, req.VehicleIds, entity.FromStationId);

                    await _repository.ClearDispatchRelationsAsync(entity.Id);

                    var newStaffs = req.StaffIds.Select(staffId => new DispatchRequestStaff
                    {
                        Id = Guid.NewGuid(),
                        DispatchRequestId = entity.Id,
                        StaffId = staffId
                    }).ToList();

                    var newVehicles = req.VehicleIds.Select(vehicleId => new DispatchRequestVehicle
                    {
                        Id = Guid.NewGuid(),
                        DispatchRequestId = entity.Id,
                        VehicleId = vehicleId
                    }).ToList();

                    await _repository.AddDispatchRelationsAsync(newStaffs, newVehicles);

                    entity.ApprovedAdminId = currentAdminId;
                    entity.Status = (int)DispatchRequestStatus.Approved;
                    break;

                case DispatchRequestStatus.ConfirmApproved:
                    DispatchValidationHelper.EnsureCanUpdate(
                        currentAdminStationId,
                        entity.ToStationId,
                        currentStatus,
                        [DispatchRequestStatus.Approved],
                        Message.UserMessage.DoNotHavePermission,
                        Message.DispatchMessage.OnlyApproveCanConfirm);

                    entity.Status = (int)DispatchRequestStatus.ConfirmApproved;
                    break;

                case DispatchRequestStatus.Received:
                    DispatchValidationHelper.EnsureCanUpdate(
                        currentAdminStationId,
                        entity.ToStationId,
                        currentStatus,
                        [DispatchRequestStatus.ConfirmApproved],
                        Message.UserMessage.DoNotHavePermission,
                        Message.DispatchMessage.OnlyConfirmCanReceive);

                    entity.Status = (int)DispatchRequestStatus.Received;

                    await _staffRepository.UpdateStationForDispatchAsync(entity.Id, entity.ToStationId);
                    await _vehicleRepository.UpdateStationForDispatchAsync(entity.Id, entity.ToStationId);
                    break;

                case DispatchRequestStatus.Cancelled:
                    DispatchValidationHelper.EnsureCanUpdate(
                        currentAdminStationId,
                        entity.ToStationId,
                        currentStatus,
                        [DispatchRequestStatus.Pending, DispatchRequestStatus.Approved],
                        Message.UserMessage.DoNotHavePermission,
                        Message.DispatchMessage.OnlyPendingCanCancel);

                    entity.Status = (int)DispatchRequestStatus.Cancelled;
                    break;

                case DispatchRequestStatus.Rejected:
                    DispatchValidationHelper.EnsureCanUpdate(
                        currentAdminStationId,
                        entity.FromStationId,
                        currentStatus,
                        [DispatchRequestStatus.Pending],
                        Message.UserMessage.DoNotHavePermission,
                        Message.DispatchMessage.OnlyPendingCanApproveReject);

                    entity.Status = (int)DispatchRequestStatus.Rejected;
                    entity.ApprovedAdminId = null;
                    entity.Description = DispatchValidationHelper.AppendDescription(
                        entity.Description, req.Description);
                    break;

                default:
                    throw new BadRequestException(Message.DispatchMessage.InvalidStatus);
            }

            entity.UpdatedAt = DateTimeOffset.UtcNow;
            await _repository.UpdateAsync(entity);
        }
    }
}