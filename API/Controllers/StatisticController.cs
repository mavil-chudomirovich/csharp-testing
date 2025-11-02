using API.Filters;
using Application;
using Application.Abstractions;
using Application.Constants;
using Application.Dtos.Common.Request;
using Application.Dtos.Statistic.Responses;
using Application.Repositories;
using Microsoft.AspNetCore.Mvc;
using System.IdentityModel.Tokens.Jwt;

namespace API.Controllers
{
    /// <summary>
    /// Controller for retrieving various statistics related to customers, revenue, invoices, and vehicles.
    /// </summary>
    [Route("api/statistic")]
    [ApiController]
    [RoleAuthorize(RoleName.Admin)]
    public class StatisticController(IStatisticService statisticService, IStaffRepository staffRepository) : ControllerBase
    {
        private readonly IStatisticService _statisticService = statisticService;
        private readonly IStaffRepository _staffRepository = staffRepository;

        private async Task<Guid?> GetCurrentStationIdAsync()
        {
            var userId = Guid.Parse(User.FindFirst(JwtRegisteredClaimNames.Sid)!.Value);
            var staff = await _staffRepository.GetByUserIdAsync(userId);
            return staff?.StationId;
        }

        /// <summary>
        /// Get statistics for registered customers this and last month.
        /// </summary>
        [HttpGet("customers")]
        public async Task<ActionResult<CustomerRes>> GetCustomers([FromQuery] PaginationParams pagination)
        {
            var result = await _statisticService.GetCustomer(pagination);
            return Ok(result);
        }

        /// <summary>
        /// Get statistics for anonymous customers (no email).
        /// </summary>
        [HttpGet("customers/anonymous")]
        public async Task<ActionResult<CustomerAnonymusRes>> GetAnonymousCustomers([FromQuery] PaginationParams pagination)
        {
            var result = await _statisticService.GetAnonymusCustomer(pagination);
            return Ok(result);
        }

        /// <summary>
        /// Get revenue summary for this and last month.
        /// </summary>
        [HttpGet("revenue")]
        public async Task<ActionResult<TotalRevenueRes>> GetTotalRevenue([FromQuery] PaginationParams pagination)
        {
            var stationId = await GetCurrentStationIdAsync();
            var result = await _statisticService.GetTotalRevenue(stationId, pagination);
            return Ok(result);
        }

        /// <summary>
        /// Get total invoices created for this and last month.
        /// </summary>
        [HttpGet("invoices")]
        public async Task<ActionResult<TotalStatisticRes>> GetTotalStatistic([FromQuery] PaginationParams pagination)
        {
            var stationId = await GetCurrentStationIdAsync();
            var result = await _statisticService.GetTotalStatistic(stationId, pagination);
            return Ok(result);
        }

        /// <summary>
        /// Get total number of vehicles by status for current station.
        /// </summary>
        [HttpGet("vehicles")]
        public async Task<ActionResult<VehicleTotalRes>> GetVehicleTotal()
        {
            var stationId = await GetCurrentStationIdAsync();
            var result = await _statisticService.GetVehicleTotal(stationId);
            return Ok(result);
        }
        /// <summary>
        /// Get total number of vehicles by status for current station.
        /// </summary>
        [HttpGet("vehicle-models")]
        public async Task<ActionResult> GetVehicleModelTotal()
        {
            var stationId = await GetCurrentStationIdAsync();
            var result = await _statisticService.GetVehicleModelTotal(stationId);
            return Ok(result == null ? [] : result.VehicleModelsForStatisticRes);
        }

        /// <summary>
        /// Get total revenue for each month in a specific year.
        /// </summary>
        /// <param name="year">
        /// The target year to calculate revenue.  
        /// If not provided, defaults to the current year.
        /// </param>
        /// <returns>List of months with total revenue per month.</returns>
        [HttpGet("revenue-by-year")]
        public async Task<IActionResult> GetRevenueByYear([FromQuery] int? year)
        {
            var stationId = await GetCurrentStationIdAsync();
            var targetYear = year ?? DateTime.UtcNow.Year; 

            var result = await _statisticService.GetRevenueByYear(stationId, targetYear);
            return Ok(result);
        }
    }
}
