using API.Filters;
using Application;
using Application.Abstractions;
using Application.Constants;
using Application.Dtos.Common.Request;
using Application.Dtos.VehicleChecklist.Request;
using Application.Dtos.VehicleChecklistItem.Request;
using Application.Dtos.VehicleModel.Respone;
using Domain.Entities;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers
{
    /// <summary>
    /// This controller manages vehicle checklists.
    /// </summary>
    [Route("api/vehicle-checklists")]
    [ApiController]
    public class VehicleChecklistController(IVehicleChecklistService vehicleChecklistService, IChecklistItemImageService imageService) : ControllerBase
    {
        private readonly IVehicleChecklistService _vehicleChecklistService = vehicleChecklistService;
        private readonly IChecklistItemImageService _imageService = imageService;

        /// <summary>
        /// Creates a new vehicle checklist for a rental contract (staff only).
        /// </summary>
        /// <param name="req">Request containing vehicle checklist details such as contract ID, vehicle condition, and checklist items.</param>
        /// <returns>The unique identifier of the created vehicle checklist.</returns>
        /// <response code="200">Success.</response>
        /// <response code="404">Vehicle or rental contract not found.</response>
        [HttpPost]
        [RoleAuthorize(RoleName.Staff)]
        public async Task<IActionResult> CreateVehicleChecklist(CreateVehicleChecklistReq req)
        {
            var staff = HttpContext.User;
            var id = await _vehicleChecklistService.Create(staff, req);
            return Ok(new { id });
        }


        /// <summary>
        /// Updates an existing vehicle checklist (staff only).
        /// </summary>
        /// <param name="req">Request containing updated checklist information, including item statuses and notes.</param>
        /// <param name="id">The unique identifier of the vehicle checklist to update.</param>
        /// <returns>Success message if the checklist is updated successfully.</returns>
        /// <response code="200">Success.</response>
        /// <response code="401">Unauthorized — user is not authenticated.</response>
        /// <response code="403">Forbidden — user does not have permission to update this checklist.</response>
        /// <response code="404">Vehicle checklist not found.</response>
        [HttpPut("{id}")]
        [RoleAuthorize(RoleName.Staff)]
        public async Task<IActionResult> UpdateVehicleChecklist([FromBody] UpdateVehicleChecklistReq req, Guid id)
        {
            await _vehicleChecklistService.UpdateAsync(req, id);
            return Ok();
        }

        /// <summary>
        /// Updates a specific item within a vehicle checklist (staff only).
        /// </summary>
        /// <param name="id">The unique identifier of the checklist item to update.</param>
        /// <param name="req">Request containing the updated status and notes for the checklist item.</param>
        /// <returns>Success message if the checklist item is updated successfully.</returns>
        /// <response code="200">Success.</response>
        /// <response code="401">Unauthorized — user is not authenticated.</response>
        /// <response code="403">Forbidden — user does not have permission to update this checklist item.</response>
        /// <response code="404">Checklist item not found.</response>
        [HttpPut("items/{id}")]
        [RoleAuthorize(RoleName.Staff)]
        public async Task<IActionResult> UpdateVehicleChecklistItems(Guid id, UpdateChecklistItemReq req)
        {
            await _vehicleChecklistService.UpdateItemsAsync(id, req.Status, req.Notes);
            return Ok();

        }
        /// <summary>
        /// Retrieves a vehicle checklist by its unique identifier (accessible by staff and customers).
        /// </summary>
        /// <param name="id">The unique identifier of the vehicle checklist.</param>
        /// <returns>Vehicle checklist details including items, status, and related information.</returns>
        /// <response code="200">Success.</response>
        /// <response code="404">Vehicle checklist not found.</response>
        [HttpGet("{id}")]
        [RoleAuthorize(RoleName.Staff, RoleName.Customer)]
        public async Task<IActionResult> GetById(Guid id)
        {
            var user = HttpContext.User;
            var checklistViewRes = await _vehicleChecklistService.GetByIdAsync(id, user);
            return Ok(checklistViewRes);
        }

        /// <summary>
        /// Retrieves all vehicle checklists, optionally filtered by contract ID or checklist type (accessible by staff and customers).
        /// </summary>
        /// <param name="contractId">Optional filter for the rental contract ID.</param>
        /// <param name="type">Optional filter for the checklist type (e.g., handover, return).</param>
        /// <param name="pagination">option pagiantion.</param>
        /// <returns>List of vehicle checklists matching the specified filters.</returns>
        /// <response code="200">Success.</response>
        /// <response code="404">No vehicle checklists found.</response>
        [HttpGet]
        [RoleAuthorize(RoleName.Staff, RoleName.Customer)]
        public async Task<IActionResult> GetAll([FromQuery] Guid? contractId, [FromQuery] int? type, [FromQuery] PaginationParams pagination)
        {
            var user = HttpContext.User;
            var checklistsViewRes = await _vehicleChecklistService.GetAllPagination(contractId, type, user, pagination);
            return Ok(checklistsViewRes);
        }

        /// <summary>
        /// Uploads an image for a specific vehicle checklist item (staff only).
        /// </summary>
        /// <param name="itemId">The unique identifier of the checklist item.</param>
        /// <param name="file">The image file to upload for the checklist item.</param>
        /// <returns>The uploaded image information.</returns>
        /// <response code="200">Success.</response>
        /// <response code="400">Invalid file format or upload error.</response>
        /// <response code="401">Unauthorized — user is not authenticated.</response>
        /// <response code="404">Checklist item not found.</response>
        [HttpPost("items/{itemId}/image")]
        [RoleAuthorize(RoleName.Staff)]
        [ApiExplorerSettings(IgnoreApi = true)]
        [Consumes("multipart/form-data")]
        public async Task<IActionResult> UploadChecklistItemImage(Guid itemId, [FromForm(Name = "file")] IFormFile file)
        {
            var img = await _imageService.UploadChecklistItemImageAsync(itemId, file);
            return Ok(new { img });
        }

        /// <summary>
        /// Deletes the image associated with a specific vehicle checklist item (staff only).
        /// </summary>
        /// <param name="itemId">The unique identifier of the checklist item whose image will be deleted.</param>
        /// <returns>Success message if the image is deleted successfully.</returns>
        /// <response code="200">Success.</response>
        /// <response code="401">Unauthorized — user is not authenticated.</response>
        /// <response code="403">Forbidden — user does not have permission to delete this image.</response>
        /// <response code="404">Checklist item or image not found.</response>
        [HttpDelete("items/{itemId}/image")]
        [RoleAuthorize(RoleName.Staff)]
        public async Task<IActionResult> DeleteChecklistItemImage(Guid itemId)
        {
            var result = await _imageService.DeleteChecklistItemImageAsync(itemId);
            return Ok(result);
        }

        /// <summary>
        /// Update customer sign.
        /// </summary>
        /// <param name="id">The checklist id.</param>
        /// <returns>Success message if the image is deleted successfully.</returns>
        /// <response code="200">Success.</response>
        /// <response code="401">Unauthorized — user is not authenticated.</response>
        /// <response code="403">Forbidden — user does not have permission to delete this image.</response>
        /// <response code="404">Checklist item or image not found.</response>
        [HttpPut("{id}/customer-sign")]
        [RoleAuthorize(RoleName.Customer)]
        public async Task<IActionResult> CustomerSignVehicleChecklist(Guid id)
        {
            var user = HttpContext.User;
            await _vehicleChecklistService.SignByCustomerAsync(id, user);
            return Ok();
        }
    }
}