using API.Filters;
using Application;
using Application.Abstractions;
using Application.Constants;
using Application.Dtos.VehicleSegment.Request;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers
{
    /// <summary>
    /// Handles operations related to vehicle segments such as listing, creation, and updates.
    /// </summary>
    [Route("api/vehicle-segments")]
    [ApiController]
    public class VehicleSegmentController(IVehicleSegmentService vehicleSegmentSerivce) : ControllerBase
    {
        private readonly IVehicleSegmentService _vehicleSegmentSerivce = vehicleSegmentSerivce;

        /// <summary>
        /// Retrieves all vehicle segments available in the system.
        /// </summary>
        /// <returns>List of vehicle segments.</returns>
        /// <response code="200">Success.</response>
        /// <response code="404">No vehicle segments found.</response>
        [HttpGet]
        public async Task<IActionResult> GetAllVehicleSegment()
        {
            var vehicleSegments = await _vehicleSegmentSerivce.GetAllVehicleSegment();
            return Ok(vehicleSegments);
        }

        /// <summary>
        /// Retrieves all vehicle segments available in the system.
        /// </summary>
        /// <param name="id">Segment id</param>
        /// <returns>List of vehicle segments.</returns>
        /// <response code="200">Success.</response>
        /// <response code="404">No vehicle segments found.</response>
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            var vehicleSegment = await _vehicleSegmentSerivce.GetByIdAsync(id);
            return Ok(vehicleSegment);
        }

        /// <summary>
        /// Create vehicle segment.
        /// </summary>
        /// <param name="req">Create segment request object.</param>
        /// <returns>List of vehicle segments.</returns>
        /// <response code="200">Success.</response>
        /// <response code="400">Bad request.</response>
        /// <response code="409">duplicate name.</response>
        [HttpPost]
        [RoleAuthorize(RoleName.SuperAdmin)]
        public async Task<IActionResult> CreateSegment([FromBody] CreateSegmentReq req)
        {
            var id = await _vehicleSegmentSerivce.CreateAsync(req);
            return Ok(new { id = id });
        }

        /// <summary>
        /// Update vehicle segment.
        /// </summary>
        /// <param name="id">Segment id</param>
        /// <param name="req">Update segment request object.</param>
        /// <returns>List of vehicle segments.</returns>
        /// <response code="200">Success.</response>
        /// <response code="409">duplicate name.</response>
        [HttpPut("{id}")]
        [RoleAuthorize(RoleName.SuperAdmin)]
        public async Task<IActionResult> UpdateSegment([FromRoute] Guid id, [FromBody] UpdateSegmentReq req)
        {
            await _vehicleSegmentSerivce.UpdateAsync(id, req);
            return Ok();
        }

        /// <summary>
        /// Delete vehicle segment.
        /// </summary>
        /// <param name="id">Segment id</param>
        /// <returns>List of vehicle segments.</returns>
        /// <response code="200">Success.</response>
        [HttpDelete("{id}")]
        [RoleAuthorize(RoleName.SuperAdmin)]
        public async Task<IActionResult> DeleteSegment([FromRoute] Guid id)
        {
            await _vehicleSegmentSerivce.DeleteAsync(id);
            return Ok();
        }

    }
}
