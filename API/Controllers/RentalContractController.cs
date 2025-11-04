using API.Filters;
using Application.Abstractions;
using Application.AppExceptions;
using Application.Constants;
using Application.Dtos.Common.Request;
using Application.Dtos.RentalContract.Request;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.IdentityModel.Tokens.Jwt;
using System.Reflection.Metadata.Ecma335;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Security.Claims;

namespace API.Controllers
{
    /// <summary>
    ///This controller handle for rental contract.
    /// </summary>
    [Route("api/rental-contracts")]
    [ApiController]
    public class RentalContractController(IRentalContractService rentalContractService,
        IVehicleChecklistService vehicleChecklistService) : ControllerBase
    {
        private readonly IRentalContractService _rentalContractService = rentalContractService;
        private readonly IVehicleChecklistService _vehicleChecklistService = vehicleChecklistService;

        /// <summary>
        /// Creates a new rental contract for the authenticated customer.
        /// </summary>
        /// <param name="createReq">Request containing rental details such as vehicle, station, and rental period.</param>
        /// <returns>Information about the created rental contract.</returns>
        /// <response code="200">Success.</response>
        /// <response code="404">Vehicle or vehicle model not found.</response>
        /// <response code="422">Business error (invalid or missing citizen ID).</response>
        [HttpPost]
        [Authorize]
        [RoleAuthorize(RoleName.Customer)]
        public async Task<IActionResult> CreateRentalContract(CreateRentalContractReq createReq)
        {
            var userClaims = HttpContext.User;
            var userID = Guid.Parse(userClaims.FindFirstValue(JwtRegisteredClaimNames.Sid)!.ToString());
            await _rentalContractService.CreateRentalContractAsync(userID, createReq);
            return Created(
            // rentalContractViewRes
            );
        }

        /// <summary>
        /// Approves or verifies a rental contract by its unique identifier.
        /// </summary>
        /// <param name="id">The unique identifier of the rental contract.</param>
        /// <param name="req">The unique identifier of the rental contract.</param>
        /// <returns>Success message if the rental contract is verified successfully.</returns>
        /// <response code="200">Success.</response>
        /// <response code="404">Rental contract not found.</response>
        [HttpPut("{id}/confirm")]
        [RoleAuthorize(RoleName.Staff)]
        public async Task<IActionResult> ConfirmRentalContract(Guid id, [FromBody] ConfirmReq req)
        {
            await _rentalContractService.VerifyRentalContract(id, req);
            return Ok();
        }

        /// <summary>
        /// Creates a new rental contract manually (offline) for a specific customer.
        /// </summary>
        /// <param name="req">Request containing rental details such as customer, vehicle, and rental period.</param>
        /// <returns>Information about the created rental contract.</returns>
        /// <response code="200">Success.</response>
        /// <response code="404">Vehicle or vehicle model not found.</response>
        /// <response code="422">Business error (invalid or missing citizen ID).</response>
        [RoleAuthorize(RoleName.Staff)]
        [HttpPost("manual")]
        public async Task<IActionResult> CreateRentalContractOffline(CreateRentalContractReq req)
        {
            var userId = req.CustomerId
                ?? throw new BadRequestException(Message.UserMessage.UserIdIsRequired);
            await _rentalContractService.CreateRentalContractAsync((Guid)userId, req);
            return Created();
        }

        /// <summary>
        /// Retrieves all rental contracts with optional filtering and pagination.
        /// </summary>
        /// <param name="req">Request containing filter parameters.</param>
        /// <param name="pagination">Pagination parameters.</param>
        /// <returns>List of rental contracts that match the specified criteria.</returns>
        /// <response code="200">Success.</response>
        /// <response code="404">Rental contract not found.</response>
        [RoleAuthorize(RoleName.Staff)]
        [HttpGet]
        public async Task<IActionResult> GetAll(
            [FromQuery] GetAllRentalContactReq req,
            [FromQuery] PaginationParams pagination)
        {
            var result = await _rentalContractService.GetAllByPagination(req, pagination);
            return Ok(result);
        }

        /// <summary>
        /// Processes the handover of a rental contract, marking the vehicle as handed over to the customer.
        /// </summary>
        /// <param name="id">The unique identifier of the rental contract.</param>
        /// <param name="req">Request containing handover details such as checklist and vehicle condition.</param>
        /// <returns>Success message if the handover process is completed successfully.</returns>
        /// <response code="200">Success.</response>
        /// <response code="404">Rental contract not found.</response>
        /// <response code="422">Business error (invalid handover conditions).</response>
        [RoleAuthorize(RoleName.Staff)]
        [HttpPut("{id}/handover")]
        public async Task<IActionResult> HandoverRentalContract(Guid id, HandoverContractReq req)
        {
            var staff = HttpContext.User;
            await _rentalContractService.HandoverProcessRentalContractAsync(staff, id, req);
            return Ok();
        }

        /// <summary>
        /// Processes the return of a rental contract and generates the corresponding return invoice.
        /// </summary>
        /// <param name="id">The unique identifier of the rental contract.</param>
        /// <returns>The generated return invoice ID if the process is successful.</returns>
        /// <response code="200">Success.</response>
        /// <response code="404">Contract not found.</response>
        [RoleAuthorize(RoleName.Staff)]
        [HttpPut("{id}/return")]
        public async Task<IActionResult> ReturnRentalContract(Guid id)
        {
            var staff = HttpContext.User;
            var returnInvoiceId = await _rentalContractService.ReturnProcessRentalContractAsync(staff, id);
            return Ok(returnInvoiceId);
        }

        /// <summary>
        /// Retrieves all rental contracts of the authenticated customer, optionally filtered by status.
        /// </summary>
        /// <param name="status">Optional status filter for the rental contracts.</param>
        /// <param name="stationId">Optional stationId filter for the rental contracts.</param>
        /// <param name="pagination">Optional pagination filter for the rental contracts.</param>
        /// <returns>List of the customer's rental contracts.</returns>
        /// <response code="200">Success.</response>
        /// <response code="404">No rental contracts found for the customer.</response>
        [RoleAuthorize(RoleName.Customer)]
        [HttpGet("me")]
        public async Task<IActionResult> GetMyContracts(
            [FromQuery] PaginationParams pagination,
            [FromQuery] int? status,
            [FromQuery] Guid? stationId)
        {
            var user = HttpContext.User;
            var result = await _rentalContractService.GetMyContractsByPagination(user, pagination, status, stationId);
            return Ok(result);
        }

        /// <summary>
        /// Updates the status of a rental contract by its unique identifier.
        /// </summary>
        /// <param name="id">The unique identifier of the rental contract.</param>
        /// <returns>Success message if the rental contract status is updated successfully.</returns>
        /// <response code="200">Success.</response>
        /// <response code="404">Rental contract not found.</response>
        [Authorize]
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateRentalContractStatus(Guid id)
        {
            await _rentalContractService.UpdateStatusAsync(id);
            return Ok();
        }

        /// <summary>
        /// Retrieves a rental contract by its unique identifier.
        /// </summary>
        /// <param name="id">The unique identifier of the rental contract.</param>
        /// <returns>Rental contract details if found.</returns>
        /// <response code="200">Success.</response>
        /// <response code="404">Rental contract not found.</response>
        [Authorize]
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            var contractView = await _rentalContractService.GetByIdAsync(id);
            return Ok(contractView);
        }

        /// <summary>
        /// Cancels a rental contract by its unique identifier if it is still eligible for cancellation.
        /// </summary>
        /// <param name="id">The unique identifier of the rental contract.</param>
        /// <returns>Success message if the rental contract is canceled successfully.</returns>
        /// <response code="200">Success.</response>
        /// <response code="400">Rental contract not found.</response>
        /// <response code="404">Bad request — this contract cannot be canceled.</response>
        [RoleAuthorize(RoleName.Customer)]
        [HttpPut("{id}/cancel")]
        public async Task<IActionResult> CancelRentalContract(Guid id)
        {
            await _rentalContractService.CancelRentalContract(id);
            return Ok();
        }

        /// <summary>
        /// Processes a vehicle change request for a specific rental contract.
        /// </summary>
        /// <param name="id">The unique identifier of the rental contract.</param>
        /// <returns>Success message if the vehicle change is processed successfully.</returns>
        /// <response code="200">Vehicle change processed successfully.</response>
        /// <response code="404">Rental contract not found.</response>
        /// <response code="403">Access denied. Only staff can perform this action.</response>
        [HttpPut("{id}/change-vehicle")]
        [RoleAuthorize(RoleName.Staff)]
        public async Task<IActionResult> ProcessChangeVehicle(Guid id)
        {
            await _rentalContractService.ChangeVehicleAsync(id);
            return Ok();
        }

        /// <summary>
        /// Handles the customer's confirmation or resolution for a rental contract issue.
        /// </summary>
        /// <param name="id">The unique identifier of the rental contract.</param>
        /// <param name="req">The customer's resolution option request.</param>
        /// <returns>Success message if the confirmation is processed successfully.</returns>
        /// <response code="200">Customer confirmation processed successfully.</response>
        /// <response code="400">Invalid resolution option.</response>
        /// <response code="404">Rental contract not found.</response>
        /// <response code="403">User does not have permission to perform this action.</response>
        [HttpPut("{id}/customer-confirm")]
        [RoleAuthorize(RoleName.Customer)]
        public async Task<IActionResult> ProcessCustomerConfirm(Guid id, CustomerResolutionOptionReq req)
        {
            await _rentalContractService.ProcessCustomerConfirm(id, req.ResolutionOption);
            return Ok();
        }
    }
}