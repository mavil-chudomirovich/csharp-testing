using System.Text;
using System.Text.Json;
using Application.Abstractions;
using Application.AppExceptions;
using Application.Constants;
using Application.Dtos.Common.Request;
using Application.Dtos.Dispatch.Request;
using Application.Dtos.Dispatch.Response;
using Application.Dtos.Station.Respone;
using Application.Helpers;
using Application.Repositories;
using AutoMapper;
using Domain.Commons;
using Domain.Entities;
using Google.Apis.Upload;
using Microsoft.AspNetCore.Components;

namespace Application
{
    public class DispatchService : IDispatchRequestService
    {
        private readonly IDispatchRepository _repository;
        private readonly IMapper _mapper;
        private readonly IVehicleRepository _vehicleRepository;
        private readonly IStaffRepository _staffRepository;
        private readonly IVehicleModelRepository _vehicleModelRepository;
        private readonly IStationRepository _stationRepository;

        public DispatchService(
            IDispatchRepository repository,
            IMapper mapper,
            IVehicleRepository vehicleRepository,
            IStaffRepository staffRepository,
            IVehicleModelRepository vehicleModelRepository,
            IStationRepository stationRepository)
        {
            _repository = repository;
            _mapper = mapper;
            _vehicleRepository = vehicleRepository;
            _staffRepository = staffRepository;
            _vehicleModelRepository = vehicleModelRepository;
            _stationRepository = stationRepository;
        }

        // ================= CREATE =================
        public async Task<Guid> CreateAsync(Guid adminId, CreateDispatchReq req)
        {
            if (req is null)
                throw new BadRequestException(Message.DispatchMessage.InvalidStatus);

            var requestAdminStaff = await _staffRepository.GetByUserIdAsync(adminId)
                ?? throw new ForbidenException(Message.UserMessage.DoNotHavePermission);

            //var fromStationId = req.FromStationId;
            //var toStationId = requestAdminStaff.StationId;

            //DispatchValidationHelper.EnsureDifferentStations(fromStationId, toStationId);

            // Validate staff
            //if (req.NumberOfStaff is > 0)
            //{
            //    var availableStaffCount =
            //        await _staffRepository.CountAvailableStaffInStationAsync(fromStationId);

            //    if (req.NumberOfStaff > availableStaffCount)
            //        throw new BadRequestException(Message.DispatchMessage.StaffNotEnoughtInFromStation);

            //    if (req.NumberOfStaff == availableStaffCount)
            //        throw new BadRequestException(Message.DispatchMessage.StaffLimitInFromStation);
            //}

            // Validate vehicles
            //if (req.Vehicles is { Length: > 0 })
            //{
            //    foreach (var v in req.Vehicles)
            //    {
            //        var availableVehicles = await _vehicleRepository
            //            .CountAvailableVehiclesByModelAsync(fromStationId, v.ModelId);

            //        if (availableVehicles < v.NumberOfVehicle)
            //            throw new BadRequestException(Message.DispatchMessage.VehicleOrStaffNotInFromStation);

            //        if (availableVehicles == v.NumberOfVehicle)
            //            throw new BadRequestException(Message.DispatchMessage.VehicleLimitInFromStation);
            //    }
            //}

            // Build description DTO
            var descriptionDto = new DispatchDescriptionDto
            {
                NumberOfStaffs = req.NumberOfStaffs ?? 0,
                Vehicles = []
            };

            if (req.Vehicles is { Length: > 0 })
            {
                foreach (var v in req.Vehicles)
                {
                    var model = await _vehicleModelRepository.GetByIdAsync(v.ModelId)
                        ?? throw new NotFoundException(Message.VehicleModelMessage.NotFound);

                    descriptionDto.Vehicles.Add(new DispatchDescriptionVehicleDto
                    {
                        ModelId = v.ModelId,
                        ModelName = model.Name,
                        Quantity = v.Quantity
                    });
                }
            }

            var entity = new DispatchRequest
            {
                Id = Guid.NewGuid(),
                RequestAdminId = adminId,
                //FromStationId = fromStationId,
                ToStationId = requestAdminStaff.StationId,
                Status = (int)DispatchRequestStatus.Pending,
                Description = JsonSerializer.Serialize(descriptionDto)
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

        public async Task<IEnumerable<StationForDispatchRes>> GetValidStationWithDescription(Guid id)
        {
            var entity = await _repository.GetByIdAsync(id)
                ?? throw new NotFoundException(Message.DispatchMessage.NotFound);
            var description = JsonHelper.DeserializeJSON<DispatchDescriptionDto>(entity.Description)
                ?? throw new JsonException(Message.JsonMessage.ParsingFailed);

            var stations = (await _stationRepository.GetAllAsync()).Where(s => s.Id != entity.ToStationId);
            var validStations = new List<StationForDispatchRes>();
            if (description.NumberOfStaffs > 0 || (description.Vehicles != null && description.Vehicles.Count > 0))
            {
                foreach (var station in stations)
                {
                    try
                    {
                        var availableDescription = await ValidateNumberOfStaffsAndVehiclesAsync(entity.Description, station.Id);
                        validStations.Add(new StationForDispatchRes
                        {
                            Id = station.Id,
                            Name = station.Name,
                            Address = station.Address,
                            AvailableDescription = availableDescription
                        });
                    }
                    catch (Exception)
                    {
                        continue;
                    }
                }
            }
            return validStations;
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

            switch ((DispatchRequestStatus)req.Status)
            {
                case DispatchRequestStatus.Assigned:
                    DispatchValidationHelper.EnsureCanUpdate(
                        currentAdminStationId,
                        entity.FromStationId!.Value,
                        currentStatus,
                        [DispatchRequestStatus.Approved],
                        Message.UserMessage.DoNotHavePermission,
                        Message.DispatchMessage.OnlyApproveCanAssign);

                    // check if any required field is null
                    if (entity.FromStationId == null)
                        throw new Exception(Message.DispatchMessage.FromStationIsRequire);
                    if (entity.FinalDescription == null)
                        throw new Exception(Message.DispatchMessage.FinalDescriptionIsRequire);

                    // parse final description to object
                    var requireDescription = JsonHelper.DeserializeJSON<DispatchDescriptionDto>(entity.FinalDescription)
                        ?? throw new Exception(Message.JsonMessage.ParsingFailed);

                    // validate number of staffs
                    var staffs = await _staffRepository.GetByIdsAsync(req.StaffIds);
                    var filteredStaffs = staffs
                        .Where(s => s.StationId == entity.FromStationId)
                        .ToArray();

                    if (requireDescription.NumberOfStaffs != filteredStaffs.Length)
                        throw new BadRequestException(Message.DispatchMessage.InvalidNumberOfStaffs);

                    // validate number of vehicles
                    var vehicles = await _vehicleRepository.GetByIdsAsync(req.VehicleIds);
                    var formatedVehicles = vehicles
                        .Where(v => v.Status == (int)VehicleStatus.Available
                            && v.StationId == entity.FromStationId)
                        .GroupBy(v => v.ModelId)
                        .Select(g => new VehicleDispatchReq
                        {
                            ModelId = g.Key,
                            Quantity = g.Count()
                        })
                        .ToArray();
                    if (requireDescription.Vehicles != null && requireDescription.Vehicles.Count > 0)
                    {
                        foreach (var v in requireDescription.Vehicles)
                        {
                            var selectedVehicle = formatedVehicles
                                .FirstOrDefault(x => x.ModelId == v.ModelId);
                            if (selectedVehicle == null || v.Quantity != selectedVehicle.Quantity)
                                throw new BadRequestException(Message.DispatchMessage.InvalidNumberOfVehicles);
                        }

                        var extraModels = formatedVehicles
                            .Select(x => x.ModelId)
                            .Except(requireDescription.Vehicles.Select(v => v.ModelId))
                            .ToList();

                        if (extraModels.Count != 0)
                        {
                            throw new BadRequestException(Message.DispatchMessage.InvalidNumberOfVehicles);
                        }
                    }

                    // clear old relations
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
                    entity.Status = (int)DispatchRequestStatus.Assigned;
                    break;

                case DispatchRequestStatus.Received:
                    DispatchValidationHelper.EnsureCanUpdate(
                        currentAdminStationId,
                        entity.ToStationId,
                        currentStatus,
                        [DispatchRequestStatus.Assigned],
                        Message.UserMessage.DoNotHavePermission,
                        Message.DispatchMessage.OnlyAssignCanReceive);

                    entity.Status = (int)DispatchRequestStatus.Received;

                    await _staffRepository.UpdateStationForDispatchAsync(entity.Id, entity.ToStationId);
                    await _vehicleRepository.UpdateStationForDispatchAsync(entity.Id, entity.ToStationId);
                    break;

                case DispatchRequestStatus.Cancelled:
                    DispatchValidationHelper.EnsureCanUpdate(
                        currentAdminStationId,
                        entity.ToStationId,
                        currentStatus,
                        [DispatchRequestStatus.Pending],
                        Message.UserMessage.DoNotHavePermission,
                        Message.DispatchMessage.OnlyPendingCanCancel);

                    entity.Status = (int)DispatchRequestStatus.Cancelled;
                    break;

                default:
                    throw new BadRequestException(Message.DispatchMessage.InvalidStatus);
            }

            await _repository.UpdateAsync(entity);
        }

        public async Task ConfirmAsync(Guid id, ConfirmDispatchReq req)
        {
            var dispatch = await _repository.GetByIdAsync(id)
                ?? throw new NotFoundException(Message.DispatchMessage.NotFound);

            var description = JsonHelper.DeserializeJSON<DispatchDescriptionDto>(dispatch.Description)
                ?? throw new JsonException(Message.JsonMessage.ParsingFailed);

            switch ((DispatchRequestStatus)req.Status)
            {
                case DispatchRequestStatus.Approved:
                    {
                        if (dispatch.Status != (int)DispatchRequestStatus.Pending)
                            throw new BadRequestException(Message.DispatchMessage.OnlyPendingCanApproveReject);

                        if (req.FinalDescription == null)
                            throw new BadRequestException(Message.DispatchMessage.FinalDescriptionIsRequire);

                        if (req.FinalDescription.NumberOfStaffs > description.NumberOfStaffs)
                            throw new BadRequestException(Message.DispatchMessage.InvalidNumberOfStaffs);

                        if (req.FinalDescription.Vehicles != null && req.FinalDescription.Vehicles.Count > 0)
                        {
                            if (description.Vehicles == null
                                || description.Vehicles.Count == 0
                                || req.FinalDescription.Vehicles.Count != description.Vehicles.Count
                            )
                                throw new BadRequestException(Message.DispatchMessage.InvalidNumberOfVehicles);

                            foreach (var v in req.FinalDescription.Vehicles)
                            {
                                var originVehicle = description.Vehicles
                                    .FirstOrDefault(x => x.ModelId == v.ModelId)
                                    ?? throw new BadRequestException(Message.DispatchMessage.InvalidNumberOfVehicles);

                                if (v.Quantity > originVehicle.Quantity)
                                    throw new BadRequestException(Message.DispatchMessage.InvalidNumberOfVehicles);
                            }
                        }

                        if (req.FinalDescription.NumberOfStaffs == 0 &&
                            (req.FinalDescription.Vehicles == null ||
                             req.FinalDescription.Vehicles.All(v => v.Quantity == 0)))
                        {
                            throw new BadRequestException(Message.DispatchMessage.NoStaffNoVehicleReject);
                        }

                        var rawFinalDesc = JsonSerializer.Serialize(req.FinalDescription);
                        await ValidateNumberOfStaffsAndVehiclesAsync(
                            rawFinalDesc,
                            req.FromStationId!.Value
                        );

                        dispatch.FromStationId = req.FromStationId;
                        dispatch.FinalDescription = rawFinalDesc;
                        break;
                    }
                case DispatchRequestStatus.Rejected:
                    {
                        if (dispatch.Status != (int)DispatchRequestStatus.Pending)
                            throw new BadRequestException(Message.DispatchMessage.OnlyPendingCanApproveReject);
                        break;
                    }
                default:
                    throw new BadRequestException(Message.DispatchMessage.InvalidStatus);
            }

            dispatch.Status = req.Status;
            await _repository.UpdateAsync(dispatch);
        }

        private async Task<DispatchDescriptionDto> ValidateNumberOfStaffsAndVehiclesAsync(string? rawDispatchDescription, Guid fromStationId)
        {
            var description = JsonHelper.DeserializeJSON<DispatchDescriptionDto>(rawDispatchDescription)
                ?? throw new JsonException(Message.JsonMessage.ParsingFailed);

            var availableDescription = new DispatchDescriptionDto
            {
                NumberOfStaffs = 0,
                Vehicles = []
            };

            //Validate staff
            if (description.NumberOfStaffs > 0)
            {
                var availableStaffCount = await _staffRepository.CountAvailableStaffInStationAsync(fromStationId);
                if (description.NumberOfStaffs > availableStaffCount)
                    throw new BadRequestException(Message.DispatchMessage.StaffNotEnoughtInFromStation);

                if (description.NumberOfStaffs == availableStaffCount)
                    throw new BadRequestException(Message.DispatchMessage.StaffLimitInFromStation);

                availableDescription.NumberOfStaffs = availableStaffCount;
            }

            //Validate vehicles
            if (description.Vehicles != null && description.Vehicles.Count > 0)
            {
                var vehicles = await _vehicleRepository.GetAllAsync(fromStationId, (int)VehicleStatus.Available);
                var formatVehicles = vehicles
                    .Where(v => v.Status == (int)VehicleStatus.Available)
                    .GroupBy(v => v.ModelId)
                    .Select(g => new VehicleDispatchReq
                    {
                        ModelId = g.Key,
                        Quantity = g.Count()
                    })
                    .ToArray();

                foreach (var v in description.Vehicles)
                {
                    var availableVehicles = formatVehicles.FirstOrDefault(x => x.ModelId == v.ModelId)?.Quantity ?? 0;

                    if (v.Quantity > availableVehicles)
                        throw new BadRequestException(Message.DispatchMessage.VehicleNotEnoughtInFromStation);

                    //if (v.Quantity == availableVehicles)
                    //    throw new BadRequestException(Message.DispatchMessage.VehicleLimitInFromStation);

                    availableDescription.Vehicles.Add(new DispatchDescriptionVehicleDto
                    {
                        ModelId = v.ModelId,
                        ModelName = v.ModelName,
                        Quantity = availableVehicles
                    });
                }
            }

            return availableDescription;
        }
    }
}