using API.Filters;
using Application.Abstractions;
using Application.Constants;
using Application.Dtos.Common.Request;
using Application.Dtos.Ticket.Request;
using Microsoft.AspNetCore.Mvc;
using System.IdentityModel.Tokens.Jwt;

namespace API.Controllers
{
    /// <summary>
    /// Manages ticket-related operations such as creation, retrieval, and updates.
    /// </summary>
    [ApiController]
    [Route("api/tickets")]
    public class TicketController(ITicketService service) : ControllerBase
    {
        private readonly ITicketService _service = service;

        /// <summary>
        /// Creates a new contact ticket for guest.
        /// </summary>
        /// <param name="req">Request containing ticket title, description.</param>
        /// <returns>The unique identifier of the created ticket.</returns>
        /// <response code="200">Success — ticket created.</response>
        [HttpPost("contact")]
        public async Task<IActionResult> CreateContact([FromBody] CreateContactReq req)
        {
            var id = await _service.CreateContactAsync(req);
            return Ok(new { Id = id });
        }

        // ==========
        // for customer
        // ==========

        #region customer

        /// <summary>
        /// Creates a new support ticket for the authenticated user.
        /// </summary>
        /// <param name="req">Request containing ticket title, description, and related information.</param>
        /// <returns>The unique identifier of the created ticket.</returns>
        /// <response code="200">Success — ticket created.</response>
        /// <response code="400">Invalid ticket data.</response>
        /// <response code="404">Related entity not found (e.g., station or contract).</response>
        [HttpPost]
        [RoleAuthorize([RoleName.Staff, RoleName.Customer])]
        public async Task<IActionResult> Create([FromBody] CreateTicketReq req)
        {
            var userIdClaim = User.FindFirst(JwtRegisteredClaimNames.Sid);
            Guid? userId = userIdClaim != null ? Guid.Parse(userIdClaim.Value) : null;
            var id = await _service.CreateAsync(userId, req);
            return Ok(new { Id = id });
        }

        /// <summary>
        /// Retrieves all support tickets created by the authenticated customer.
        /// </summary>
        /// <param name="status">Status filter parameters.</param>
        /// <param name="pagination">Pagination parameters.</param>
        /// <returns>List of tickets submitted by the current customer.</returns>
        /// <response code="200">Success.</response>
        /// <response code="404">No tickets found for the customer.</response>
        [HttpGet("me")]
        [RoleAuthorize([RoleName.Staff, RoleName.Customer])]
        public async Task<IActionResult> GetMyTickets(
            [FromQuery] int? status,
            [FromQuery] PaginationParams pagination
        )
        {
            var userId = Guid.Parse(User.FindFirst(JwtRegisteredClaimNames.Sid)!.Value);
            var data = await _service.GetByCustomerAsync(userId, status, pagination);
            return Ok(data);
        }

        #endregion customer

        // ===================
        // for staff and admin
        // ===================

        #region management

        /// <summary>
        /// Retrieves all support tickets with pagination support (for admin and staff roles).
        /// </summary>
        /// <param name="filter">Filter parameters.</param>
        /// <param name="pagination">Pagination parameters.</param>
        /// <returns>List of support tickets with pagination metadata.</returns>
        /// <response code="200">Success.</response>
        /// <response code="404">No tickets found.</response>
        [HttpGet]
        [RoleAuthorize([RoleName.Admin, RoleName.Staff])]
        public async Task<IActionResult> GetAll(
            [FromQuery] TicketFilterParams filter,
            [FromQuery] PaginationParams pagination
        )
        {
            var userId = Guid.Parse(User.FindFirst(JwtRegisteredClaimNames.Sid)!.Value);
            var data = await _service.GetAllAsync(userId, filter, pagination);
            return Ok(data);
        }

        /// <summary>
        /// Updates an existing support ticket with new information (for admin and staff roles).
        /// </summary>
        /// <param name="id">The unique identifier of the ticket to update.</param>
        /// <param name="req">Request containing updated ticket details such as status, notes, or assigned staff.</param>
        /// <returns>No content if the ticket is updated successfully.</returns>
        /// <response code="204">Success — ticket updated.</response>
        /// <response code="400">Invalid update data.</response>
        /// <response code="404">Ticket not found.</response>
        [HttpPatch("{id}")]
        [RoleAuthorize([RoleName.Admin, RoleName.Staff])]
        public async Task<IActionResult> Update(Guid id, [FromBody] UpdateTicketReq req)
        {
            var staffId = Guid.Parse(User.FindFirst(JwtRegisteredClaimNames.Sid)!.Value);
            await _service.UpdateAsync(id, req, staffId);
            return Ok();
        }

        #region escalated

        /// <summary>
        /// Escalates a specific ticket to the admin for further review or action.
        /// </summary>
        /// <param name="id">The unique identifier of the ticket to escalate.</param>
        /// <returns>Success message if the escalation is completed successfully.</returns>
        /// <response code="200">Ticket escalated to admin successfully.</response>
        /// <response code="404">Ticket not found.</response>
        /// <response code="403">Access denied. Only staff can perform this action.</response>
        [HttpPatch("{id}/escalated-to-admin")]
        [RoleAuthorize(RoleName.Staff)]
        public async Task<IActionResult> EscalateToAdmin(Guid id)
        {
            var staffId = Guid.Parse(User.FindFirst(JwtRegisteredClaimNames.Sid)!.Value);
            await _service.EscalateToAdminAsync(id);
            return Ok();
        }

        /// <summary>
        /// Retrieves all tickets that have been escalated to the admin, with pagination support.
        /// </summary>
        /// <param name="pagination">Pagination parameters for filtering and page size.</param>
        /// <returns>A paginated list of escalated tickets.</returns>
        /// <response code="200">Escalated tickets retrieved successfully.</response>
        /// <response code="403">Access denied. Only admins can perform this action.</response>
        [HttpGet("escalated")]
        [RoleAuthorize(RoleName.Admin)]
        public async Task<IActionResult> GetEscalatedTickets([FromQuery] PaginationParams pagination)
        {
            var data = await _service.GetEscalatedTicketsAsync(pagination);
            return Ok(data);
        }

        #endregion escalated

        #endregion management
    }
}