using Application.Dtos.Common.Request;
using Application.Dtos.Statistic.Responses;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Abstractions
{
    public interface IStatisticService 
    {
        Task<CustomerRes?> GetCustomer([FromQuery] PaginationParams pagination);
        Task<CustomerAnonymusRes?> GetAnonymusCustomer([FromQuery] PaginationParams pagination);
        Task<TotalRevenueRes?> GetTotalRevenue(Guid? stationId, [FromQuery] PaginationParams pagination);
        Task<TotalStatisticRes?> GetTotalStatistic(Guid? stationId, [FromQuery] PaginationParams pagination);
        Task<VehicleTotalRes?> GetVehicleTotal(Guid? stationId);
        Task<VehicleModelsStatisticRes?> GetVehicleModelTotal(Guid? stationId);
        Task<IEnumerable<RevenueByMonthRes>> GetRevenueByYear(Guid? stationId, int year);
    }
}
