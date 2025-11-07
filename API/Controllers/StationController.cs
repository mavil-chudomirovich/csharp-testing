using API.Filters;
using Application;
using Application.Abstractions;
using Application.Constants;
using Application.Dtos.Station.Request;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers
{
    /// <summary>
    /// Manages station-related operations such as listing, creating, and updating stations.
    /// </summary>
    [Route("api/stations")]
    [ApiController]
    public class StationController(IStationService stationService) : ControllerBase
    {
        private readonly IStationService _stationService = stationService;

        /// <summary>
        /// Retrieves all stations available in the system.
        /// </summary>
        /// <returns>List of all stations.</returns>
        /// <response code="200">Success.</response>
        /// <response code="404">No stations found.</response>
        [HttpGet]
        public async Task<IActionResult> GetAllStation()
        {
            var stations = await _stationService.GetAllStation();
            return Ok(stations);
        }

        /// <summary>
        /// Retrieves a specific station by its identifier.
        /// </summary>
        /// <param name="id">The unique identifier of the station.</param>
        /// <returns>The station information.</returns>
        /// <response code="200">Station found.</response>
        /// <response code="404">Station not found.</response>
        [HttpGet("{id:guid}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            var station = await _stationService.GetByIdAsync(id);
            return Ok(station);
        }

        /// <summary>
        /// Creates a new station.
        /// </summary>
        /// <param name="request">The data required to create a station.</param>
        /// <returns>The created station.</returns>
        /// <response code="201">Station created successfully.</response>
        [HttpPost]
        [RoleAuthorize(RoleName.SuperAdmin)]
        public async Task<IActionResult> Create([FromBody] StationCreateReq request)
        {
            var created = await _stationService.CreateAsync(request);
            return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
        }

        /// <summary>
        /// Updates an existing station by its identifier.
        /// </summary>
        /// <param name="id">The ID of the station to update.</param>
        /// <param name="request">The updated station data.</param>
        /// <returns>The updated station.</returns>
        /// <response code="200">Station updated successfully.</response>
        /// <response code="404">Station not found.</response>
        [HttpPut("{id:guid}")]
        [RoleAuthorize(RoleName.SuperAdmin)]
        public async Task<IActionResult> Update(Guid id, [FromBody] StationUpdateReq request)
        {
            request.Id = id;
            var updated = await _stationService.UpdateAsync(request);
            return Ok(updated);
        }

        /// <summary>
        /// Soft-deletes a station (marks it as deleted without removing it from the database).
        /// </summary>
        /// <param name="id">The ID of the station to delete.</param>
        /// <response code="204">Station deleted successfully.</response>
        /// <response code="404">Station not found.</response>
        [HttpDelete("{id:guid}")]
        [RoleAuthorize(RoleName.SuperAdmin)]
        public async Task<IActionResult> Delete(Guid id)
        {
            await _stationService.DeleteAsync(id);
            return Ok();
        }
    }
}
