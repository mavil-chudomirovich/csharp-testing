using API.Filters;
using Application.Abstractions;
using Application.AppExceptions;
using Application.Constants;
using Application.Dtos.Dispatch.Request;
using Application.Repositories;
using Microsoft.AspNetCore.Mvc;
using System.IdentityModel.Tokens.Jwt;

namespace API.Controllers
{
    /// <summary>
    /// Handles all dispatch request operations such as creating, assigning,
    /// and managing dispatch requests between staff and users.
    /// </summary>
    [ApiController]
    [Route("api/dispatch-requests")]
    public class DispatchRequestController(IDispatchRequestService dispatchRequestService, IUserService userService, IStaffRepository staffRepository) : ControllerBase
    {
        private readonly IDispatchRequestService _dispatchRequestService = dispatchRequestService;
        private readonly IUserService _userService = userService;
        private readonly IStaffRepository _staffRepository = staffRepository;

        /// <summary>
        /// Creates a new dispatch request (admin only).
        /// </summary>
        /// <param name="req">Request containing dispatch details such as vehicle, source station, destination station, and assigned staff.</param>
        /// <returns>The unique identifier of the created dispatch request.</returns>
        /// <response code="200">Success.</response>
        /// <response code="400">Invalid dispatch request data.</response>
        /// <response code="401">Unauthorized — user is not authenticated.</response>
        /// <response code="403">Forbidden — user does not have permission to perform this action.</response>
        [HttpPost]
        [RoleAuthorize(RoleName.Admin)]
        public async Task<IActionResult> Create([FromBody] CreateDispatchReq req)
        {
            var adminId = Guid.Parse(User.FindFirst(JwtRegisteredClaimNames.Sid)!.Value);
            var dispatchId = await _dispatchRequestService.CreateAsync(adminId, req);
            return Ok(new { dispatchId });
        }

        /// <summary>
        /// Assign a station to a dispatch request (super admin only).
        /// </summary>
        /// <param name="id">The unique identifier of the dispatch request to update.</param>
        /// <param name="req">Request containing the assign station id.</param>
        /// <returns>Success message if the dispatch request status is updated successfully.</returns>
        /// <response code="200">Success.</response>
        /// <response code="400">Invalid status update data.</response>
        /// <response code="401">Unauthorized — user is not authenticated.</response>
        /// <response code="403">Forbidden — user does not have permission to perform this action.</response>
        /// <response code="404">Dispatch request not found.</response>
        [HttpPut("{id:guid}/confirm")]
        [RoleAuthorize(RoleName.SuperAdmin)]
        public async Task<IActionResult> Confirm([FromRoute] Guid id, [FromBody] ConfirmDispatchReq req)
        {
            await _dispatchRequestService.ConfirmAsync(id, req);
            return Ok();
        }

        /// <summary>
        /// Updates the status of a dispatch request (accessible by staff or admin).
        /// </summary>
        /// <param name="id">The unique identifier of the dispatch request to update.</param>
        /// <param name="req">Request containing the new status and any related update details.</param>
        /// <returns>Success message if the dispatch request status is updated successfully.</returns>
        /// <response code="200">Success.</response>
        /// <response code="400">Invalid status update data.</response>
        /// <response code="401">Unauthorized — user is not authenticated.</response>
        /// <response code="403">Forbidden — user does not have permission to perform this action.</response>
        /// <response code="404">Dispatch request not found.</response>
        [HttpPut("{id:guid}")]
        [RoleAuthorize(RoleName.Admin, RoleName.SuperAdmin)]
        public async Task<IActionResult> Update([FromRoute] Guid id, [FromBody] UpdateDispatchReq req)
        {
            var userId = Guid.Parse(User.FindFirst(JwtRegisteredClaimNames.Sid)!.Value);
            var staff = await _staffRepository.GetByUserIdAsync(userId)
                ?? throw new ForbidenException(Message.UserMessage.DoNotHavePermission);

            await _dispatchRequestService.UpdateAsync(userId, staff.StationId, id, req);
            return Ok();
        }

        /// <summary>
        /// Retrieves all dispatch requests with optional filters for source station, destination station, and status.
        /// </summary>
        /// <param name="fromStationId">Optional filter for the source station identifier.</param>
        /// <param name="toStationId">Optional filter for the destination station identifier.</param>
        /// <param name="status">Optional filter for the dispatch request status.</param>
        /// <returns>List of dispatch requests matching the specified filters.</returns>
        /// <response code="200">Success.</response>
        /// <response code="401">Unauthorized — user is not authenticated.</response>
        /// <response code="403">Forbidden — user does not have permission to perform this action.</response>
        [HttpGet]
        [RoleAuthorize(RoleName.Admin, RoleName.SuperAdmin)]
        public async Task<IActionResult> GetAll(
            [FromQuery] Guid? fromStationId,
            [FromQuery] Guid? toStationId,
            [FromQuery] DispatchRequestStatus? status)
        {
            var result = await _dispatchRequestService.GetAllAsync(fromStationId, toStationId, status);
            return Ok(result);
        }

        /// <summary>
        /// Retrieves detailed information about a specific dispatch request by its unique identifier.
        /// </summary>
        /// <param name="id">The unique identifier of the dispatch request.</param>
        /// <returns>Detailed dispatch request information if found.</returns>
        /// <response code="200">Success.</response>
        /// <response code="404">Dispatch request not found.</response>
        [HttpGet("{id:guid}")]
        [RoleAuthorize(RoleName.Admin, RoleName.SuperAdmin)]
        public async Task<IActionResult> GetById([FromRoute] Guid id)
        {
            var res = await _dispatchRequestService.GetByIdAsync(id);
            if (res == null)
                return NotFound();

            return Ok(res);
        }

        /// <summary>
        /// Get valid stations for dispatching a specific dispatch request (super admin only).
        /// </summary>
        /// <param name="id">The unique identifier of the dispatch request.</param>
        /// <returns>Valid stations for the description dispatch</returns>
        /// <response code="200">Success.</response>
        [HttpGet("{id:guid}/valid-stations")]
        [RoleAuthorize(RoleName.SuperAdmin)]
        public async Task<IActionResult> GetValidStationsForDispatch([FromRoute] Guid id)
        {
            var stations = await _dispatchRequestService.GetValidStationWithDescription(id);
            return Ok(stations);
        }
    }
}