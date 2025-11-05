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
    [RoleAuthorize(RoleName.SuperAdmin, RoleName.Admin)]
    public class StatisticController(IStatisticService statisticService, IStaffRepository staffRepository) : ControllerBase
    {
        private readonly IStatisticService _statisticService = statisticService;
        private readonly IStaffRepository _staffRepository = staffRepository;


        /// <summary>
        /// Get statistics for registered customers this and last month.
        /// </summary>
        [HttpGet("customers")]
        public async Task<ActionResult<CustomerRes>> GetCustomers()
        {
            var result = await _statisticService.GetCustomer();
            return Ok(result);
        }

        /// <summary>
        /// Get statistics for anonymous customers (no email).
        /// </summary>
        [HttpGet("customers/anonymous")]
        public async Task<ActionResult<CustomerAnonymusRes>> GetAnonymousCustomers()
        {
            var result = await _statisticService.GetAnonymusCustomer();
            return Ok(result);
        }

        /// <summary>
        /// Get revenue summary for this and last month.
        /// </summary>
        [HttpGet("revenue/{stationId}")]
        public async Task<ActionResult<TotalRevenueRes>> GetTotalRevenue(Guid? stationId)
        {
            var result = await _statisticService.GetTotalRevenue(stationId);
            return Ok(result);
        }

        /// <summary>
        /// Get total invoices created for this and last month.
        /// </summary>
        [HttpGet("invoices/{stationId}")]
        public async Task<ActionResult<TotalStatisticRes>> GetTotalStatistic(Guid? stationId)
        {
            var result = await _statisticService.GetTotalStatistic(stationId);
            return Ok(result);
        }

        /// <summary>
        /// Get total number of vehicles by status for current station.
        /// </summary>
        [HttpGet("vehicles/{stationId}")]
        public async Task<ActionResult<VehicleTotalRes>> GetVehicleTotal(Guid? stationId)
        {
            var result = await _statisticService.GetVehicleTotal(stationId);
            return Ok(result);
        }
        /// <summary>
        /// Get total number of vehicles by status for current station.
        /// </summary>
        [HttpGet("vehicle-models/{stationId}")]
        public async Task<ActionResult> GetVehicleModelTotal(Guid? stationId)
        {
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
        /// <param name="stationId"></param>
        /// <returns>List of months with total revenue per month.</returns>
        [HttpGet("revenue-by-year/{stationId}")]
        public async Task<IActionResult> GetRevenueByYear([FromQuery] int? year, Guid? stationId)
        {
            var targetYear = year ?? DateTime.UtcNow.Year; 

            var result = await _statisticService.GetRevenueByYear(stationId, targetYear);
            return Ok(result);
        }

        /// <summary>
        /// Get total invoice for each month in a specific year.
        /// </summary>
        /// <param name="year">
        /// The target year to calculate revenue.  
        /// If not provided, defaults to the current year.
        /// </param>
        /// <param name="stationId"></param>
        /// <returns>List of months with total revenue per month.</returns>
        [HttpGet("invoice-by-year/{stationId}")]
        public async Task<IActionResult> GetInvoiceByYear([FromQuery] int? year, Guid? stationId)
        {
            var targetYear = year ?? DateTime.UtcNow.Year; 

            var result = await _statisticService.GetInvoiceByYear(stationId, targetYear);
            return Ok(result);
        }
    }
}
