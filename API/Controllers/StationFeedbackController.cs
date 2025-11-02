using API.Filters;
using Application.Abstractions;
using Application.Constants;
using Application.Dtos.Common.Request;
using Application.Dtos.StationFeedback.Request;
using Microsoft.AspNetCore.Mvc;
using System.IdentityModel.Tokens.Jwt;

namespace API.Controllers
{
    /// <summary>
    /// Handles operations related to station feedback such as creating, viewing, and managing feedback.
    /// </summary>
    [ApiController]
    [Route("api/station-feedbacks")]
    public class StationFeedbackController(IStationFeedbackService service) : ControllerBase
    {

        /// <summary>
        /// Creates a new feedback entry for a specific station from the authenticated customer.
        /// </summary>
        /// <param name="req">Request containing station ID, rating, and feedback content.</param>
        /// <returns>Feedback details if creation is successful.</returns>
        /// <response code="200">Success.</response>
        /// <response code="400">Invalid feedback data.</response>
        /// <response code="404">Station not found.</response>
        /// <response code="404">Station not found.</response>
        [HttpPost]
        [RoleAuthorize(RoleName.Customer)]
        public async Task<IActionResult> Create([FromBody] StationFeedbackCreateReq req)
        {
            var customerId = Guid.Parse(User.FindFirst(JwtRegisteredClaimNames.Sid)!.Value);
            var result = await service.CreateAsync(req, customerId);
            return Ok(result);
        }

        /// <summary>
        /// Retrieves all feedback entries for a specific station by its unique identifier.
        /// </summary>
        /// <param name="id">The unique identifier of the station.</param>
        /// <returns>List of feedback entries related to the specified station.</returns>
        /// <response code="200">Success.</response>
        /// <response code="404">Station not found or no feedback available.</response>
        [HttpGet("station/{id}")]
        public async Task<IActionResult> GetByStationId(Guid id)
        {
            var data = await service.GetByStationIdAsync(id);
            return Ok(data);
        }

        /// <summary>
        /// Retrieves all feedback entries submitted by the authenticated customer.
        /// </summary>
        /// <returns>List of feedbacks created by the current customer.</returns>
        /// <response code="200">Success.</response>
        [HttpGet("me")]
        public async Task<IActionResult> GetMyFeedbacks()
        {
            var customerId = Guid.Parse(User.FindFirst(JwtRegisteredClaimNames.Sid)!.Value);
            var data = await service.GetByCustomerIdAsync(customerId);
            return Ok(data);
        }

        /// <summary>
        /// Deletes a specific feedback entry created by the authenticated customer.
        /// </summary>
        /// <param name="id">The unique identifier of the feedback to delete.</param>
        /// <returns>No content if the feedback is deleted successfully.</returns>
        /// <response code="204">Success — feedback deleted.</response>
        /// <response code="403">Forbidden — customer does not have permission to delete this feedback.</response>
        [HttpDelete("{id}")]
        [RoleAuthorize(RoleName.Staff)]
        public async Task<IActionResult> Delete(Guid id)
        {
            await service.DeleteAsync(id);
            return Ok();
        }

        /// <summary>
        /// Retrieves all station feedback records.
        /// </summary>
        /// <param name="stationId">Optional filter for station ID.</param>
        /// <param name="pagination">Pagination parameters.</param>
        /// <returns>A list of all station feedbacks.</returns>
        /// <response code="200">Feedbacks retrieved successfully.</response>
        [HttpGet]
        public async Task<IActionResult> GetAllFeedbacks(
            [FromQuery] PaginationParams pagination,
            [FromQuery] Guid? stationId)
        {
            var data = await service.GetAllAsync(pagination, stationId);
            return Ok(data);
        }
    }
}