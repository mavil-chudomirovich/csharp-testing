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
    public class StatisticController(IStatisticService statisticService) : ControllerBase
    {
        private readonly IStatisticService _statisticService = statisticService;

        /// <summary>
        /// Get statistics for registered customers this and last month.
        /// </summary>
        [HttpGet("customers")]
        public async Task<ActionResult> GetCustomers()
        {
            var result = await _statisticService.GetCustomer();
            return Ok(result);
        }

        /// <summary>
        /// Get statistics for anonymous customers (no email).
        /// </summary>
        [HttpGet("customers/anonymous")]
        public async Task<ActionResult> GetAnonymousCustomers()
        {
            var result = await _statisticService.GetAnonymusCustomer();
            return Ok(result);
        }

        /// <summary>
        /// Get revenue summary for this and last month.
        /// </summary>
        [HttpGet("revenue")]
        public async Task<ActionResult> GetTotalRevenue([FromQuery] Guid? stationId)
        {
            var result = await _statisticService.GetTotalRevenue(stationId);
            return Ok(result);
        }

        /// <summary>
        /// Get total invoices created for this and last month.
        /// </summary>
        [HttpGet("invoices")]
        public async Task<ActionResult> GetTotalInvoice([FromQuery] Guid? stationId)
        {
            var result = await _statisticService.GetTotalInvoice(stationId);
            return Ok(result);
        }

        /// <summary>
        /// Get total contracts created for this and last month.
        /// </summary>
        [HttpGet("contracts")]
        public async Task<ActionResult> GetTotalContracts([FromQuery] Guid? stationId)
        {
            var result = await _statisticService.GetTotalContracts(stationId);
            return Ok(result);
        }

        /// <summary>
        /// Get total number of vehicles by status for current station.
        /// </summary>
        [HttpGet("vehicles")]
        public async Task<ActionResult> GetVehicleTotal([FromQuery] Guid? stationId)
        {
            var result = await _statisticService.GetVehicleTotal(stationId);
            return Ok(result);
        }

        /// <summary>
        /// Get total number of vehicles by status for current station.
        /// </summary>
        [HttpGet("vehicle-models")]
        public async Task<ActionResult> GetVehicleModelTotal([FromQuery] Guid? stationId)
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
        [HttpGet("revenue-by-year")]
        public async Task<IActionResult> GetRevenueByYear([FromQuery] int? year, [FromQuery] Guid? stationId)
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
        [HttpGet("invoice-by-year")]
        public async Task<IActionResult> GetInvoiceByYear([FromQuery] int? year, [FromQuery] Guid? stationId)
        {
            var targetYear = year ?? DateTime.UtcNow.Year;

            var result = await _statisticService.GetInvoiceByYear(stationId, targetYear);
            return Ok(result);
        }

        /// <summary>
        /// Get total contracts for each month in a specific year.
        /// </summary>
        /// <param name="year">
        /// The target year to calculate contracts.
        /// If not provided, defaults to the current year.
        /// </param>
        /// <param name="stationId">
        /// Optional station ID.
        /// If not provided, returns contracts from all stations.
        /// </param>
        /// <returns>List of months with total contracts per month.</returns>
        [HttpGet("contract-by-year")]
        public async Task<IActionResult> GetContractByYear([FromQuery] int? year, [FromQuery] Guid? stationId)
        {
            var targetYear = year ?? DateTime.UtcNow.Year;

            var result = await _statisticService.GetContractByYear(stationId, targetYear);
            return Ok(result);
        }
    }
}