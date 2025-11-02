using Application.AppExceptions;
using Application.Constants;
using Application.Dtos.Common.Request;
using Application.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Helpers
{
    public static class DispatchValidationHelper
    {
        public static async Task ValidateStaffsInStationAsync(
            IStaffRepository staffRepository, 
            Guid[]? staffIds, 
            Guid expectedStationId)
        {
            if (staffIds == null || staffIds.Length == 0) return;
            staffIds = staffIds.Where(x => x != Guid.Empty).ToArray();
            var countValid = await staffRepository.CountStaffsInStationAsync(staffIds, expectedStationId);
            if (countValid != staffIds.Length)
                throw new BadRequestException(Message.DispatchMessage.StaffNotInFromStation);
        }


        public static async Task ValidateVehiclesInStationAsync(IVehicleRepository vehicleRepository, Guid[]? vehicleId, Guid fromStationId)
        {
            if (vehicleId == null || vehicleId.Length == 0) return;
            vehicleId = vehicleId.Where(x => x != Guid.Empty).ToArray();
            var countValid = await vehicleRepository.CountVehiclesInStationAsync(vehicleId, fromStationId);
            if (countValid != vehicleId.Length)
                throw new BadRequestException(Message.DispatchMessage.VehicleNotInFromStation);
        }

        public static void EnsureDifferentStations(Guid fromStationId, Guid toStationId)
        {
            if (fromStationId == toStationId)
                throw new ForbidenException(Message.DispatchMessage.ToStationMustDifferent);
        }

        public static void EnsureCanUpdate(
            Guid currentStationId,
            Guid expectedStationId,
            DispatchRequestStatus currentStatus,
            DispatchRequestStatus[] requiredStatus,
            string forbiddenMessage,
            string invalidStatusMessage)
        {
            if (currentStationId != expectedStationId)
                throw new ForbidenException(forbiddenMessage);
            if (!requiredStatus.Contains(currentStatus))
                throw new BadRequestException(invalidStatusMessage);
        }


        public static async Task ValidateStaffQuantityAsync(
            IStaffRepository staffRepository,
            Guid stationId,
            int? numberRequired)
        {
            if (!numberRequired.HasValue || numberRequired.Value <= 0) return;

            var available = await staffRepository.CountAvailableStaffInStationAsync(stationId);
            if (available < numberRequired.Value)
                throw new BadRequestException(Message.DispatchMessage.StaffNotInFromStation);
        }
        public static async Task ValidateVehicleQuantityByModelAsync(
            IVehicleRepository vehicleRepository,
            Guid stationId,
            IEnumerable<VehicleDispatchReq> vehicles)
        {
            if (vehicles == null) return;

            foreach (var v in vehicles)
            {
                var available = await vehicleRepository.CountAvailableVehiclesByModelAsync(stationId, v.ModelId);
                if (available < v.NumberOfVehicle)
                    throw new BadRequestException(
                        $"{Message.DispatchMessage.VehicleNotInFromStation} - model {v.ModelId} has only {available} available.");
            }
        }
        public static string AppendDescription(string? oldDesc, string? newDesc)
        {
            if (string.IsNullOrWhiteSpace(newDesc)) return oldDesc ?? "";
            if (string.IsNullOrWhiteSpace(oldDesc)) return newDesc;
            return $"{oldDesc}\n{newDesc}";
        }
    }
}