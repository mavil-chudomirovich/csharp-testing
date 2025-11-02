using Application.Dtos.VehicleModel.Request;
using Application.Dtos.VehicleModel.Respone;
using Domain.Entities;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Abstractions
{
    public interface IVehicleModelService
    {
        Task<Guid> CreateVehicleModelAsync(CreateVehicleModelReq createVehicleModelReq);

        Task<IEnumerable<VehicleModelViewRes>> SearchVehicleModel(VehicleFilterReq vehicleFilterReq);

        Task<int> UpdateVehicleModelAsync(Guid Id, UpdateVehicleModelReq updateVehicleModelReq);

        Task<bool> DeleteVehicleModleAsync(Guid id);

        Task<VehicleModelViewRes> GetByIdAsync(Guid id, Guid stationId, DateTimeOffset startDate, DateTimeOffset endDate);

        Task<string> UploadMainImageAsync(Guid modelId, IFormFile file);
        Task<IEnumerable<VehicleModelViewRes>> GetAllAsync(string? name, Guid? segmentId);
        Task DeleteMainImageAsync(Guid modelId);
        Task UpdateVehicleModelComponentsAsync(Guid id, UpdateModelComponentsReq req);
        Task<IEnumerable<VehicleModelViewRes>> GetAllAsync();
    }
}