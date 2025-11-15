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
        Task<TotalStatisticRes<int>?> GetCustomer();

        Task<TotalStatisticRes<int>?> GetAnonymusCustomer();

        Task<TotalStatisticRes<decimal>?> GetTotalRevenue(Guid? stationId);

        Task<TotalStatisticRes<int>?> GetTotalInvoice(Guid? stationId);

        Task<TotalStatisticRes<int>?> GetTotalContracts(Guid? stationId);

        Task<VehicleTotalRes?> GetVehicleTotal(Guid? stationId);

        Task<VehicleModelsStatisticRes?> GetVehicleModelTotal(Guid? stationId);

        Task<IEnumerable<RevenueByMonthRes>> GetRevenueByYear(Guid? stationId, int year);

        Task<IEnumerable<InvoiceByMonthRes>> GetInvoiceByYear(Guid? stationId, int year);

        Task<IEnumerable<ContractByMonthRes>> GetContractByYear(Guid? stationId, int year);
    }
}