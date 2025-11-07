using API.Filters;
using Application.Abstractions;
using Application.Constants;
using Application.Dtos.Common.Request;
using Application.Dtos.Vehicle.Request;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers
{
    /// <summary>
    /// Handles vehicle-related operations such as creation, updates, and retrieval.
    /// </summary>
    [Route("api/vehicles")]
    [ApiController]
    public class VehicleController(IVehicleService vehicleService) : ControllerBase
    {
        private readonly IVehicleService _vehicleService = vehicleService;

        /// <summary>
        /// Creates a new vehicle entry in the system (admin only).
        /// </summary>
        /// <param name="createVehicleReq">Request containing vehicle details such as model, station, license plate, and status.</param>
        /// <returns>The unique identifier of the created vehicle.</returns>
        /// <response code="200">Success.</response>
        /// <response code="400">Invalid vehicle data.</response>
        /// <response code="401">Unauthorized — user is not authenticated.</response>
        /// <response code="403">Forbidden — user does not have permission to perform this action.</response>
        [HttpPost]
        [RoleAuthorize(RoleName.Admin)]
        public async Task<IActionResult> CreateVehicle(CreateVehicleReq createVehicleReq)
        {
            var vehicleId = await _vehicleService.CreateVehicleAsync(createVehicleReq);
            return Ok(new
            {
                VehicleId = vehicleId
            });
        }

        /// <summary>
        /// Updates the details of an existing vehicle by its unique identifier (admin or staff only).
        /// </summary>
        /// <param name="id">The unique identifier of the vehicle to update.</param>
        /// <param name="updateVehicleReq">Request containing updated vehicle details such as status, station, or specifications.</param>
        /// <returns>Success message if the vehicle is updated successfully.</returns>
        /// <response code="200">Success.</response>
        /// <response code="401">Unauthorized — user is not authenticated.</response>
        /// <response code="403">Forbidden — user does not have permission to perform this action.</response>
        /// <response code="404">Vehicle does not exist.</response>
        [RoleAuthorize(RoleName.Staff, RoleName.Admin)]
        [HttpPatch("{id}")]
        public async Task<IActionResult> UpdateVehicle([FromRoute] Guid id, UpdateVehicleReq updateVehicleReq)
        {
            await _vehicleService.UpdateVehicleAsync(id, updateVehicleReq);
            return Ok();
        }

        /// <summary>
        /// Deletes a vehicle by its unique identifier (admin only).
        /// </summary>
        /// <param name="id">The unique identifier of the vehicle to delete.</param>
        /// <returns>Success message if the vehicle is deleted successfully.</returns>
        /// <response code="200">Success.</response>
        /// <response code="401">Unauthorized — user is not authenticated.</response>
        /// <response code="403">Forbidden — user does not have permission to perform this action.</response>
        /// <response code="404">Vehicle not found.</response>
        [HttpDelete("{id}")]
        [RoleAuthorize(RoleName.Admin)]
        public async Task<IActionResult> DeleteVehicle([FromRoute] Guid id)
        {
            await _vehicleService.DeleteVehicle(id);
            return Ok();
        }

        /// <summary>
        /// Retrieves all vehicles with optional filters for name, station, status, or license plate (admin or staff only).
        /// </summary>
        /// <param name="pagination">Optional pagination for vehicles.</param>
        /// <param name="name">Optional filter for vehicle name.</param>
        /// <param name="stationId">Optional filter for the station where the vehicle is located.</param>
        /// <param name="status">Optional filter for vehicle status (e.g., available, rented, maintenance).</param>
        /// <param name="licensePlate">Optional filter for the vehicle's license plate.</param>
        /// <returns>List of vehicles matching the specified filters.</returns>
        /// <response code="200">Success.</response>
        /// <response code="401">Unauthorized — user is not authenticated.</response>
        /// <response code="403">Forbidden — user does not have permission to perform this action.</response>
        [HttpGet]
        public async Task<IActionResult> GetAll([FromQuery] PaginationParams pagination, string? name, Guid? stationId, int? status, string? licensePlate)
        {
            var vehicle = await _vehicleService.GetAllAsync(pagination, name, stationId, status, licensePlate); 
            return Ok(vehicle);
        }

        /// <summary>
        /// Retrieves detailed information about a specific vehicle by its unique identifier (admin or staff only).
        /// </summary>
        /// <param name="id">The unique identifier of the vehicle.</param>
        /// <returns>Detailed information about the specified vehicle.</returns>
        /// <response code="200">Success.</response>
        /// <response code="401">Unauthorized — user is not authenticated.</response>
        /// <response code="403">Forbidden — user does not have permission to perform this action.</response>
        /// <response code="404">Vehicle not found.</response>
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById([FromRoute] Guid id)
        {
            var vehicle = await _vehicleService.GetVehicleById(id);
            return Ok(vehicle);
        }
    }
}