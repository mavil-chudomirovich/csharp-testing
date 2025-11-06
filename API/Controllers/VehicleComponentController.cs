using API.Filters;
using Application.Abstractions;
using Application.Constants;
using Application.Dtos.Common.Request;
using Application.Dtos.VehicleComponent.Request;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers
{
    /// <summary>
    /// Handles vechile componets CRUD.
    /// </summary>
    [Route("api/vehicle-components")]
    [ApiController]
    public class VehicleComponentController(IVehicleComponentService vehicleComponentService) : ControllerBase
    {
        private readonly IVehicleComponentService _vehicleComponentService = vehicleComponentService;

        /// <summary>
        /// GetAll vehicle components.
        /// </summary>
        /// <returns>The unique identifier of the created dispatch request.</returns>
        /// <param name="modelId">Vehicle model id</param>
        /// <param name="name">Vehicle model name</param>
        /// <param name="pagination">pagination options</param>
        /// <response code="200">Success.</response>
        /// <response code="401">Unauthorized — user is not authenticated.</response>
        [HttpGet]
        public async Task<IActionResult> GetAll([FromQuery]Guid? modelId, [FromQuery] string? name, [FromQuery] PaginationParams pagination)
        {

            var vehicleComponents = await _vehicleComponentService.GetAllAsync(modelId, name, pagination);
            return Ok(vehicleComponents);
        }

        /// <summary>
        /// GetAll vehicle components.
        /// </summary>
        /// <returns>The unique identifier of the created dispatch request.</returns>
        /// <param name="id ">Vehicle component id</param>
        /// <response code="200">Success.</response>
        /// <response code="401">Unauthorized — user is not authenticated.</response>
        /// <response code="404">Vehicle component not found.</response>
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            var vehicleComponent = await _vehicleComponentService.GetByIdAsync(id);
            return Ok(vehicleComponent);
        }
        
        /// <summary>
        /// Create vehicle component.
        /// </summary>
        /// <param name="req">Create vehicle component dto</param>
        /// <returns>The unique identifier of the created dispatch request.</returns>
        /// <response code="200">Success.</response>
        /// <response code="400">Invalid type input</response>
        /// <response code="401">Unauthorized — user is not authenticated.</response>
        [HttpPost]
        [RoleAuthorize(RoleName.SuperAdmin)]
        public async Task<IActionResult> Create(CreateVehicleComponentReq req)
        {
            var id = await _vehicleComponentService.AddAsync(req);
            return Ok(new {id = id});
        }

        /// <summary>
        /// Update vehicle component.
        /// </summary>
        /// <param name="req ">Update vehicle component dto</param>
        /// <param name="id ">Vehicle component id</param>
        /// <returns>The unique identifier of the created dispatch request.</returns>
        /// <response code="200">Success.</response>
        /// <response code="400">Invalid type input</response>
        /// <response code="401">Unauthorized — user is not authenticated.</response>
        /// <response code="404">Vehicle component not found.</response>
        [HttpPut("{id}")]
        [RoleAuthorize(RoleName.SuperAdmin)]
        public async Task<IActionResult> Update(Guid id, UpdateVehicleComponentReq req)
        {
            await _vehicleComponentService.UpdateAsync(id, req);
            return Ok();
        }

        /// <summary>
        /// Delete vehicle component.
        /// </summary>
        /// <param name="id ">Vehicle component id</param>
        /// <returns>The unique identifier of the created dispatch request.</returns>
        /// <response code="200">Success.</response>
        /// <response code="401">Unauthorized — user is not authenticated.</response>
        /// <response code="404">Vehicle component not found.</response>
        [HttpDelete("{id}")]
        [RoleAuthorize(RoleName.SuperAdmin)]
        public async Task<IActionResult> Delete(Guid id)
        {
            await _vehicleComponentService.DeleteAsync(id);
            return Ok();
        }
    }
}
