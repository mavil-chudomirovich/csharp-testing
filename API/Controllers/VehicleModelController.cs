using API.Filters;
using Application;
using Application.Abstractions;
using Application.Constants;
using Application.Dtos.Common.Request;
using Application.Dtos.VehicleModel.Request;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers
{
    /// <summary>
    /// Manages vehicle model operations such as creation, updates, and image management.
    /// </summary>
    [Route("api/vehicle-models")]
    [ApiController]
    public class VehicleModelController(IVehicleModelService vehicleModelService,
        IModelImageService modelImageService) : ControllerBase
    {
        private readonly IVehicleModelService _vehicleModelService = vehicleModelService;
        private readonly IModelImageService _modelImageService = modelImageService;

        /// <summary>
        /// Creates a new vehicle model (admin only).
        /// </summary>
        /// <param name="createVehicleModelReq">Request containing vehicle model details such as name, brand, segment, and specifications.</param>
        /// <returns>The unique identifier of the created vehicle model.</returns>
        /// <response code="200">Success.</response>
        /// <response code="400">Invalid vehicle model data or type.</response>
        /// <response code="401">Unauthorized — user is not authenticated.</response>
        /// <response code="403">Forbidden — user does not have permission to perform this action.</response>

        [RoleAuthorize(RoleName.Admin)]
        [HttpPost]
        public async Task<IActionResult> CreateVehicleModel([FromBody] CreateVehicleModelReq createVehicleModelReq)
        {
            var id = await _vehicleModelService.CreateVehicleModelAsync(createVehicleModelReq);
            return Ok(new
            {
                Id = id
            });
        }

        /// <summary>
        /// Updates an existing vehicle model by its unique identifier (admin only).
        /// </summary>
        /// <param name="id">The unique identifier of the vehicle model to update.</param>
        /// <param name="updateVehicleModelReq">Request containing updated vehicle model details such as name, specifications, or type.</param>
        /// <returns>Success message if the vehicle model is updated successfully.</returns>
        /// <response code="200">Success.</response>
        /// <response code="400">Invalid vehicle model data or type.</response>
        /// <response code="401">Unauthorized — user is not authenticated.</response>
        /// <response code="403">Forbidden — user does not have permission to perform this action.</response>
        /// <response code="404">Vehicle model not found.</response>

        [RoleAuthorize(RoleName.Admin)]
        [HttpPatch("{id}")]
        public async Task<IActionResult> UpdateVehicleModel([FromRoute] Guid id, UpdateVehicleModelReq updateVehicleModelReq)
        {
            await _vehicleModelService.UpdateVehicleModelAsync(id, updateVehicleModelReq);
            return Ok();
        }

        /// <summary>
        /// Searches for vehicle models based on the provided filter criteria.
        /// </summary>
        /// <param name="vehicleFilterReq">Request containing filter parameters such as brand, segment, price range, or capacity.</param>
        /// <returns>List of vehicle models matching the filter criteria.</returns>
        /// <response code="200">Success.</response>

        [HttpGet("search")]
        public async Task<IActionResult> SearchVehicleModel([FromQuery] VehicleFilterReq vehicleFilterReq)
        {
            var verhicelModelView = await _vehicleModelService.SearchVehicleModel(vehicleFilterReq);
            return Ok(verhicelModelView);
        }

        /// <summary>
        /// Retrieves all vehicle models with optional filters for name and segment.
        /// </summary>
        /// <param name="name">Optional filter for the vehicle model name.</param>
        /// <param name="segmentId">Optional filter for the vehicle segment identifier.</param>
        /// <returns>List of vehicle models matching the specified filters.</returns>
        /// <response code="200">Success.</response>
        [HttpGet]
        public async Task<IActionResult> GetAll(string? name, Guid? segmentId)
        {
            var verhicelModelView = await _vehicleModelService.GetAllAsync(name, segmentId);
            return Ok(verhicelModelView);
        }

        /// <summary>
        /// Retrieves detailed information of a specific vehicle model by its unique identifier,
        /// including availability data for a given station and rental period.
        /// </summary>
        /// <param name="id">The unique identifier of the vehicle model.</param>
        /// <param name="stationId">The unique identifier of the station where the vehicle is located.</param>
        /// <param name="startDate">The start date of the desired rental period.</param>
        /// <param name="endDate">The end date of the desired rental period.</param>
        /// <returns>Detailed vehicle model information with availability data.</returns>
        /// <response code="200">Success.</response>
        /// <response code="404">Vehicle model not found.</response>

        [HttpGet("{id}")]
        public async Task<IActionResult> GetVehicelModelById([FromRoute] Guid id, Guid stationId,
                                                 DateTimeOffset startDate, DateTimeOffset endDate)
        {
            var verhicelModelView = await _vehicleModelService.GetByIdAsync(id, stationId, startDate, endDate);
            return Ok(verhicelModelView);
        }

        /// <summary>
        /// Deletes a vehicle model by its unique identifier (admin only).
        /// </summary>
        /// <param name="id">The unique identifier of the vehicle model to delete.</param>
        /// <returns>Success message if the vehicle model is deleted successfully.</returns>
        /// <response code="200">Success.</response>
        /// <response code="401">Unauthorized — user is not authenticated.</response>
        /// <response code="403">Forbidden — user does not have permission to perform this action.</response>
        /// <response code="404">Vehicle model not found.</response>

        [RoleAuthorize(RoleName.Admin)]
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteVehicleModel([FromRoute] Guid id)
        {
            await _vehicleModelService.DeleteVehicleModleAsync(id);
            return Ok();
        }

        // ---------- SUB-IMAGES (gallery) ----------
        /// <summary>
        /// Uploads multiple sub-images for a specific vehicle model.
        /// </summary>
        /// <param name="modelId">The unique identifier of the vehicle model to which the images belong.</param>
        /// <param name="req">Request containing one or more image files to upload.</param>
        /// <returns>List of uploaded image URLs with a success message.</returns>
        /// <response code="200">Success.</response>
        /// <response code="400">Invalid file format or upload error.</response>
        /// <response code="404">Vehicle model not found.</response>
        [HttpPost("{modelId}/sub-images")]
        [Consumes("multipart/form-data")]
        [ApiExplorerSettings(IgnoreApi = true)]
        public async Task<IActionResult> UploadSubImages([FromRoute] Guid modelId, [FromForm] UploadImagesReq req)
        {
            var res = await _modelImageService.UploadModelImagesAsync(modelId, req.Files);
            return Ok(new { data = res, message = Message.CloudinaryMessage.UploadSuccess });
        }

        /// <summary>
        /// Deletes one or more sub-images of a specific vehicle model.
        /// </summary>
        /// <param name="modelId">The unique identifier of the vehicle model whose images will be deleted.</param>
        /// <param name="req">Request containing the list of image identifiers to delete.</param>
        /// <returns>Success message if the images are deleted successfully.</returns>
        /// <response code="200">Success.</response>
        /// <response code="400">Invalid request data or image IDs.</response>
        /// <response code="404">Vehicle model or images not found.</response>
        [HttpDelete("{modelId}/sub-images")]
        [Consumes("application/json")]
        [ApiExplorerSettings(IgnoreApi = true)]
        public async Task<IActionResult> DeleteSubImages([FromRoute] Guid modelId, [FromBody] DeleteModelImagesReq req)
        {
            await _modelImageService.DeleteModelImagesAsync(modelId, req.ImageIds);
            return Ok(new { message = Message.CloudinaryMessage.DeleteSuccess });
        }

        // ---------- MAIN IMAGE ----------
        /// <summary>
        /// Uploads the main image for a specific vehicle model.
        /// </summary>
        /// <param name="modelId">The unique identifier of the vehicle model.</param>
        /// <param name="file">The image file to be uploaded as the main image.</param>
        /// <returns>The uploaded main image URL along with a success message.</returns>
        /// <response code="200">Success.</response>
        /// <response code="400">Invalid file format or upload error.</response>
        /// <response code="404">Vehicle model not found.</response>
        [HttpPost("{modelId}/main-image")]
        [Consumes("multipart/form-data")]
        [ApiExplorerSettings(IgnoreApi = true)]
        public async Task<IActionResult> UploadMainImage([FromRoute] Guid modelId, [FromForm(Name = "file")] IFormFile file)
        {
            var imageUrl = await _vehicleModelService.UploadMainImageAsync(modelId, file);
            return Ok(new { modelId, imageUrl });
        }

        /// <summary>
        /// Deletes the main image of a specific vehicle model.
        /// </summary>
        /// <param name="modelId">The unique identifier of the vehicle model whose main image will be deleted.</param>
        /// <returns>Success message if the main image is deleted successfully.</returns>
        /// <response code="200">Success.</response>
        /// <response code="404">Vehicle model or main image not found.</response>
        [HttpDelete("{modelId}/main-image")]
        [ApiExplorerSettings(IgnoreApi = true)]
        public async Task<IActionResult> DeleteMainImage([FromRoute] Guid modelId)
        {
            await _vehicleModelService.DeleteMainImageAsync(modelId);
            return Ok(new { message = Message.CloudinaryMessage.DeleteSuccess });
        }

        // ---------- MAIN + GALLERY ----------
        /// <summary>
        /// Uploads both the main image and gallery images for a specific vehicle model.
        /// </summary>
        /// <param name="modelId">The unique identifier of the vehicle model.</param>
        /// <param name="req">Request containing one or more image files to upload.</param>
        /// <returns>The uploaded main image and gallery image URLs with a success message.</returns>
        /// <response code="200">Success.</response>
        /// <response code="400">Invalid file format or upload error.</response>
        /// <response code="404">Vehicle model not found.</response>
        [HttpPost("{modelId}/images")]
        [Consumes("multipart/form-data")]
        [ApiExplorerSettings(IgnoreApi = true)]
        public async Task<IActionResult> UploadAllImages([FromRoute] Guid modelId, [FromForm] UploadImagesReq req)
        {
            var res = await _modelImageService.UploadAllModelImagesAsync(modelId, req.Files);
            return Ok(res);
        }

        /// <summary>
        /// Uploads both the main image and gallery images for a specific vehicle model.
        /// </summary>
        /// <param name="id">Model id.</param>
        /// <param name="req">Update model component req.</param>
        /// <returns>The uploaded main image and gallery image URLs with a success message.</returns>
        /// <response code="200">Success.</response>
        [RoleAuthorize(RoleName.Admin)]
        [HttpPut("{id}/components")]
        public async Task<IActionResult> UpdateVehicleModelComponents([FromRoute] Guid id, [FromBody] UpdateModelComponentsReq req)
        {
            await _vehicleModelService.UpdateVehicleModelComponentsAsync(id, req);
            return Ok();
        }

        /// <summary>
        /// Get All main image.
        /// </summary>
        /// <response code="200">Success.</response>
        [HttpGet("main-images")]
        public async Task<IActionResult> GetAllModelMainImage()
        {
            var res = await _vehicleModelService.GetAllModelMainImage();
            return Ok(res);
        }
    }
}