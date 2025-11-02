using Application.Dtos.VehicleModel.Respone;
using Microsoft.AspNetCore.Http;

namespace Application.Abstractions
{
    public interface IModelImageService
    {
        Task<VehicleModelImagesRes> UploadModelImagesAsync(Guid modelId, List<IFormFile> files);

        Task DeleteModelImagesAsync(Guid modelId, List<Guid> imageIds);

        Task<VehicleModelImagesRes> UploadAllModelImagesAsync(Guid modelId, List<IFormFile> files);
        Task<IEnumerable<VehicleModelMainImageRes>> GetAllVehicleModelMainImagesAsync();

    }
}