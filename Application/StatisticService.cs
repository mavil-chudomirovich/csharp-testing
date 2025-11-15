using Application.Abstractions;
using Application.AppExceptions;
using Application.Constants;
using Application.Dtos.Common.Request;
using Application.Dtos.Statistic.Responses;
using Application.Dtos.Vehicle.Respone;
using Application.Helpers;
using Application.Repositories;
using Domain.Commons;
using Microsoft.AspNetCore.Mvc;

namespace Application
{
    public class StatisticService : IStatisticService
    {
        private readonly IUserService _userService;
        private readonly IVehicleService _vehicleService;
        private readonly IInvoiceService _invoiceService;
        private readonly IVehicleModelService _vehicleModelService;
        private readonly IInvoiceRepository _invoiceRepository;
        private readonly IUserRepository _userRepository;
        private readonly IRentalContractRepository _rentalContractRepository;

        public StatisticService(
            IUserService userService,
            IVehicleService vehicleService,
            IInvoiceService invoiceService,
            IVehicleModelService vehicleModelService,
            IInvoiceRepository invoiceRepository,
            IUserRepository userRepository,
            IRentalContractRepository rentalContractRepository)
        {
            _userService = userService;
            _vehicleService = vehicleService;
            _invoiceService = invoiceService;
            _vehicleModelService = vehicleModelService;
            _invoiceRepository = invoiceRepository;
            _userRepository = userRepository;
            _rentalContractRepository = rentalContractRepository;
        }

        public async Task<TotalStatisticRes<int>?> GetAnonymusCustomer()
        {
            var customer = await _userRepository.GetAllAsync(RoleName.Customer);
            if (customer == null || !customer.Any())
                return new TotalStatisticRes<int>
                {
                    TotalThisMonth = 0,
                    TotalLastMonth = 0,
                    ChangeRate = 0
                };

            int lastMonth = StatisticHelper.GetLastMonth();
            int previousYear = StatisticHelper.GetLastMonthYear();

            var customerThisMonth = customer.Count(x =>
                x != null && x.CreatedAt.Month == DateTimeOffset.UtcNow.Month
                && x.CreatedAt.Year == DateTimeOffset.UtcNow.Year
                && x.Email == null);

            var customerLastMonth = customer.Count(x =>
                x != null && x.CreatedAt.Month == lastMonth
                && x.CreatedAt.Year == previousYear
                && x.Email == null);

            decimal changeRate = 0;
            if (customerLastMonth > 0)
                changeRate = ((decimal)(customerThisMonth - customerLastMonth) / customerLastMonth) * 100;
            else if (customerThisMonth > 0)
                changeRate = 100;

            return new TotalStatisticRes<int>
            {
                TotalThisMonth = customerThisMonth,
                TotalLastMonth = customerLastMonth,
                ChangeRate = Math.Round(changeRate, 2)
            };
        }

        public async Task<TotalStatisticRes<int>?> GetCustomer()
        {
            var customer = await _userRepository.GetAllAsync(RoleName.Customer);
            if (customer == null || !customer.Any())
                return new TotalStatisticRes<int>
                {
                    TotalThisMonth = 0,
                    TotalLastMonth = 0,
                    ChangeRate = 0
                };

            int lastMonth = StatisticHelper.GetLastMonth();
            int previousYear = StatisticHelper.GetLastMonthYear();

            var customerThisMonth = customer.Count(x =>
                x != null && x.CreatedAt.Month == DateTimeOffset.UtcNow.Month &&
                x.CreatedAt.Year == DateTimeOffset.UtcNow.Year);

            var customerLastMonth = customer.Count(x =>
                x != null && x.CreatedAt.Month == lastMonth &&
                x.CreatedAt.Year == previousYear);

            decimal changeRate = 0;
            if (customerLastMonth > 0)
                changeRate = ((decimal)(customerThisMonth - customerLastMonth) / customerLastMonth) * 100;
            else if (customerThisMonth > 0)
                changeRate = 100;

            return new TotalStatisticRes<int>
            {
                TotalThisMonth = customerThisMonth,
                TotalLastMonth = customerLastMonth,
                ChangeRate = Math.Round(changeRate, 2)
            };
        }

        public async Task<TotalStatisticRes<decimal>?> GetTotalRevenue(Guid? stationId)
        {
            var invoice = await _invoiceRepository.GetAllInvoicesAsync(stationId);
            if (invoice == null || !invoice.Any())
            {
                return new TotalStatisticRes<decimal>
                {
                    TotalThisMonth = 0,
                    TotalLastMonth = 0,
                    ChangeRate = 0
                };
            }

            int lastMonth = StatisticHelper.GetLastMonth();
            int previousYear = StatisticHelper.GetLastMonthYear();
            decimal totalThisMonth = 0;
            decimal totalLastMonth = 0;

            var currentMonthInvoices = invoice
                .Where(x =>
                    x.CreatedAt.Month == DateTimeOffset.Now.Month &&
                    x.CreatedAt.Year == DateTimeOffset.Now.Year
                );

            foreach (var item in currentMonthInvoices)
                totalThisMonth += InvoiceHelper.SafeCalculateTotal(item);

            var lastMonthInvoices = invoice
                .Where(x =>
                    x.CreatedAt.Month == lastMonth &&
                    x.CreatedAt.Year == previousYear
                );

            foreach (var item in lastMonthInvoices)
                totalLastMonth += InvoiceHelper.SafeCalculateTotal(item);

            decimal changeRate = 0;
            if (totalLastMonth > 0)
                changeRate = ((totalThisMonth - totalLastMonth) / totalLastMonth) * 100;
            else if (totalThisMonth > 0)
                changeRate = 100;

            return new TotalStatisticRes<decimal>
            {
                TotalThisMonth = Math.Round(totalThisMonth, 2),
                TotalLastMonth = Math.Round(totalLastMonth, 2),
                ChangeRate = Math.Round(changeRate, 2)
            };
        }

        public async Task<TotalStatisticRes<int>?> GetTotalInvoice(Guid? stationId)
        {
            var invoice = await _invoiceRepository.GetAllInvoicesAsync(stationId);
            if (invoice == null || !invoice.Any())
            {
                return new TotalStatisticRes<int>
                {
                    TotalThisMonth = 0,
                    TotalLastMonth = 0,
                    ChangeRate = 0
                };
            }

            int lastMonth = StatisticHelper.GetLastMonth();
            int previousYear = StatisticHelper.GetLastMonthYear();

            var invoiceThisMonth = invoice.Count(x =>
                x.CreatedAt.Month == DateTimeOffset.Now.Month &&
                x.CreatedAt.Year == DateTimeOffset.Now.Year
            );

            var invoiceLastMonth = invoice.Count(x =>
                x.CreatedAt.Month == lastMonth &&
                x.CreatedAt.Year == previousYear
            );

            decimal changeRate = 0;
            if (invoiceLastMonth > 0)
                changeRate = ((decimal)(invoiceThisMonth - invoiceLastMonth) / invoiceLastMonth) * 100;
            else if (invoiceThisMonth > 0)
                changeRate = 100;

            return new TotalStatisticRes<int>
            {
                TotalThisMonth = invoiceThisMonth,
                TotalLastMonth = invoiceLastMonth,
                ChangeRate = Math.Round(changeRate, 2)
            };
        }

        public async Task<TotalStatisticRes<int>?> GetTotalContracts(Guid? stationId)
        {
            var contracts = await _rentalContractRepository.GetAllRentalContractsAsync(stationId);
            if (contracts == null || !contracts.Any())
            {
                return new TotalStatisticRes<int>
                {
                    TotalThisMonth = 0,
                    TotalLastMonth = 0,
                    ChangeRate = 0
                };
            }

            int lastMonth = StatisticHelper.GetLastMonth();
            int previousYear = StatisticHelper.GetLastMonthYear();

            var totalThisMonth = contracts.Count(x =>
                x.CreatedAt.Month == DateTimeOffset.Now.Month &&
                x.CreatedAt.Year == DateTimeOffset.Now.Year
            );

            var totalLastMonth = contracts.Count(x =>
                x.CreatedAt.Month == lastMonth &&
                x.CreatedAt.Year == previousYear
            );

            decimal changeRate = 0;
            if (totalLastMonth > 0)
                changeRate = ((decimal)(totalThisMonth - totalLastMonth) / totalLastMonth) * 100;
            else if (totalThisMonth > 0)
                changeRate = 100;

            return new TotalStatisticRes<int>
            {
                TotalThisMonth = totalThisMonth,
                TotalLastMonth = totalLastMonth,
                ChangeRate = Math.Round(changeRate, 2)
            };
        }

        public async Task<VehicleModelsStatisticRes?> GetVehicleModelTotal(Guid? stationId)
        {
            var vehicles = await _vehicleService.GetAllAsync(stationId, null);
            if (vehicles == null || !vehicles.Any())
                throw new NotFoundException(Message.StatisticMessage.NoVehicleData);

            var vehicleModels = await _vehicleModelService.GetAllAsync();
            if (vehicleModels == null || !vehicleModels.Any())
                throw new NotFoundException(Message.VehicleModelMessage.NotFound);

            var groupedByModel = vehicles
                .GroupBy(v => v.ModelId)
                .ToList();

            var items = new List<VehicleModelsForStatisticRes>();

            foreach (var group in groupedByModel)
            {
                var model = vehicleModels.FirstOrDefault(m => m.Id == group.Key);
                var modelName = model?.Name ?? "Unknown Model";

                var availableCount = group.Count(v => v.Status == (int)VehicleStatus.Available);
                var rentedCount = group.Count(v => v.Status == (int)VehicleStatus.Rented);
                var maintenanceCount = group.Count(v => v.Status == (int)VehicleStatus.Maintenance);

                items.Add(new VehicleModelsForStatisticRes(
                    group.Key,
                    modelName,
                    availableCount,
                    rentedCount,
                    maintenanceCount
                ));
            }

            return new VehicleModelsStatisticRes
            {
                VehicleModelsForStatisticRes = items.ToArray()
            };
        }

        public async Task<VehicleTotalRes?> GetVehicleTotal(Guid? stationId)
        {
            var vehicle = await _vehicleService.GetAllAsync(stationId, null);
            if (vehicle == null || !vehicle.Any())
                throw new NotFoundException(Message.StatisticMessage.NoVehicleData);

            var total = vehicle.Count();
            var items = new List<VehicleStatusCountItem>();

            foreach (VehicleStatus status in Enum.GetValues(typeof(VehicleStatus)))
            {
                var vehiclesByStatus = await _vehicleService.GetAllAsync(stationId, (int)status);
                items.Add(new VehicleStatusCountItem((int)status, vehiclesByStatus.Count()));
            }

            return new VehicleTotalRes
            {
                Total = total,
                Items = items
            };
        }

        public async Task<IEnumerable<RevenueByMonthRes>> GetRevenueByYear(Guid? stationId, int year)
        {
            var invoices = await _invoiceRepository.GetAllInvoicesAsync(stationId);
            if (invoices == null || !invoices.Any())
                return Enumerable.Range(1, 12).Select(m => new RevenueByMonthRes
                {
                    MonthName = new DateTime(year, m, 1).ToString("MMMM"),
                    TotalRevenue = 0
                });

            var monthlyData = Enumerable.Range(1, 12)
            .Select(month =>
            {
                var total = invoices
                    .Where(x =>
                        x.CreatedAt.Month == month &&
                        x.CreatedAt.Year == year
                    )
                    .Sum(x => InvoiceHelper.SafeCalculateTotal(x));

                return new RevenueByMonthRes
                {
                    MonthName = new DateTime(year, month, 1).ToString("MMMM"),
                    TotalRevenue = Math.Round(total, 2)
                };
            });

            return monthlyData;
        }

        public async Task<IEnumerable<InvoiceByMonthRes>> GetInvoiceByYear(Guid? stationId, int year)
        {
            var invoices = await _invoiceRepository.GetAllInvoicesAsync(stationId);
            if (invoices == null || !invoices.Any())
                return Enumerable.Range(1, 12).Select(m => new InvoiceByMonthRes
                {
                    MonthName = new DateTime(year, m, 1).ToString("MMMM"),
                    TotalInvoice = 0
                });

            var monthlyData = Enumerable.Range(1, 12)
            .Select(month =>
            {
                var total = invoices
                .Count(x =>
                     x.CreatedAt.Month == month &&
                     x.CreatedAt.Year == year
                );

                return new InvoiceByMonthRes
                {
                    MonthName = new DateTime(year, month, 1).ToString("MMMM"),
                    TotalInvoice = total
                };
            });

            return monthlyData;
        }

        public async Task<IEnumerable<ContractByMonthRes>> GetContractByYear(Guid? stationId, int year)
        {
            var rentalContracts = await _rentalContractRepository.GetAllRentalContractsAsync(stationId);

            if (rentalContracts == null || !rentalContracts.Any())
                return Enumerable.Range(1, 12).Select(m => new ContractByMonthRes
                {
                    MonthName = new DateTime(year, m, 1).ToString("MMMM"),
                    TotalContract = 0
                });

            var monthlyData = Enumerable.Range(1, 12)
                .Select(month =>
                {
                    var total = rentalContracts.Count(x =>
                        x.CreatedAt.Month == month &&
                        x.CreatedAt.Year == year
                    );

                    return new ContractByMonthRes
                    {
                        MonthName = new DateTime(year, month, 1).ToString("MMMM"),
                        TotalContract = total
                    };
                });

            return monthlyData;
        }
    }
}