using Application.Abstractions;
using Application.AppExceptions;
using Application.Constants;
using Application.Dtos.Common.Request;
using Application.Dtos.VehicleModel.Respone;
using Application.UnitOfWorks;
using AutoMapper;
using Domain.Entities;
using Microsoft.AspNetCore.Http;

namespace Application
{
    public class ModelImageService : IModelImageService
    {
        private readonly IModelImageUow _uow;
        private readonly IPhotoService _photoService;
        private readonly IMapper _mapper;

        public ModelImageService(IModelImageUow uow,
            IPhotoService photoService,
            IMapper mapper)
        {
            _uow = uow;
            _photoService = photoService;
            _mapper = mapper;
        }

        // =================
        // Upload img
        // =================

        #region upload

        public async Task<VehicleModelImagesRes> UploadModelImagesAsync(Guid modelId, List<IFormFile> files)
        {
            if (files == null || files.Count == 0)
                throw new BadRequestException(Message.CloudinaryMessage.NotFoundObjectInFile);

            var model = await _uow.VehicleModelRepository.GetByIdAsync(modelId)
                ?? throw new NotFoundException(Message.VehicleModelMessage.NotFound);

            var uploadedImages = await UploadModelImagesHelperAsync(modelId, files);

            model.ModelImages = [.. uploadedImages];
            await _uow.ModelImageRepository.AddRangeAsync(uploadedImages);
            await _uow.SaveChangesAsync();

            return _mapper.Map<VehicleModelImagesRes>(model);
        }

        public async Task<VehicleModelImagesRes> UploadAllModelImagesAsync(Guid modelId, List<IFormFile> files)
        {
            if (files == null || files.Count == 0)
                throw new BadRequestException(Message.CloudinaryMessage.NotFoundObjectInFile);

            var model = await _uow.VehicleModelRepository.GetByIdAsync(modelId)
                ?? throw new NotFoundException(Message.VehicleModelMessage.NotFound);

            var mainFile = files.First();
            var galleryFiles = files.Skip(1).ToList();

            var mainUpload = await _photoService.UploadPhotoAsync(new UploadImageReq { File = mainFile }, $"models/{modelId}/main");

            await using var trx = await _uow.BeginTransactionAsync();

            try
            {
                model.ImageUrl = mainUpload.Url;
                model.ImagePublicId = mainUpload.PublicID;
                model.UpdatedAt = DateTimeOffset.UtcNow;

                await _uow.VehicleModelRepository.UpdateAsync(model);
                var galleryDtos = await UploadModelImagesHelperAsync(modelId, galleryFiles);
                model.ModelImages = [.. galleryDtos];
                await _uow.SaveChangesAsync();
                await trx.CommitAsync();

                return _mapper.Map<VehicleModelImagesRes>(model);
            }
            catch
            {
                await trx.RollbackAsync();

                try { await _photoService.DeletePhotoAsync(mainUpload.PublicID); } catch { }

                throw;
            }
        }

        private async Task<IEnumerable<ModelImage>> UploadModelImagesHelperAsync(Guid modelId, List<IFormFile> files)
        {
            var uploadedImages = new List<ModelImage>();

            foreach (var file in files)
            {
                var uploadReq = new UploadImageReq { File = file };
                var result = await _photoService.UploadPhotoAsync(uploadReq, $"models/{modelId}/gallery");

                if (string.IsNullOrEmpty(result.Url))
                    throw new BusinessException(Message.CloudinaryMessage.UploadFailed);

                uploadedImages.Add(new ModelImage
                {
                    Id = Guid.NewGuid(),
                    ModelId = modelId,
                    Url = result.Url,
                    PublicId = result.PublicID,
                    CreatedAt = DateTimeOffset.UtcNow,
                    UpdatedAt = DateTimeOffset.UtcNow
                });
            }
            await _uow.ModelImageRepository.AddRangeAsync(uploadedImages);
            await _uow.SaveChangesAsync();

            return uploadedImages;
        }

        #endregion upload

        // =================
        // Deleted img
        // =================
        public async Task DeleteModelImagesAsync(Guid modelId, List<Guid> imageIds)
        {
            if (imageIds == null || imageIds.Count == 0)
                throw new BadRequestException(Message.CommonMessage.UnexpectedError);

            var images = await _uow.ModelImageRepository.FindAsync(x =>
                imageIds.Contains(x.Id) && x.ModelId == modelId);

            if (!images.Any())
                throw new NotFoundException(Message.CloudinaryMessage.NotFoundObjectInFile);

            foreach (var image in images)
            {
                try
                {
                    await _photoService.DeletePhotoAsync(image.PublicId);
                }
                catch
                {
                }

                _uow.ModelImageRepository.Remove(image);
            }

            await _uow.SaveChangesAsync();
        }

        public async Task<IEnumerable<VehicleModelMainImageRes>> GetAllVehicleModelMainImagesAsync()
        {
            var models = await _uow.VehicleModelRepository.GetAllAsync();

            var result = models
                .Where(m => !string.IsNullOrEmpty(m.ImageUrl))
                .Select(m => new VehicleModelMainImageRes
                {
                    Id = m.Id,
                    Name = m.Name,
                    ImageUrl = m.ImageUrl
                })
                .ToList();

            return result;
        }
    }
}