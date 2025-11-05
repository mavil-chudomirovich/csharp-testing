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
        Task<CustomerRes?> GetCustomer();
        Task<CustomerAnonymusRes?> GetAnonymusCustomer();
        Task<TotalRevenueRes?> GetTotalRevenue(Guid? stationId);
        Task<TotalStatisticRes?> GetTotalStatistic(Guid? stationId);
        Task<VehicleTotalRes?> GetVehicleTotal(Guid? stationId);
        Task<VehicleModelsStatisticRes?> GetVehicleModelTotal(Guid? stationId);
        Task<IEnumerable<RevenueByMonthRes>> GetRevenueByYear(Guid? stationId, int year);
        Task<IEnumerable<InvoiceByMonthRes>> GetInvoiceByYear(Guid? stationId, int year);
    }
}
