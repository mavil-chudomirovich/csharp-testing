using Application.Dtos.Common.Request;
using Application.Dtos.Common.Response;
using Application.Dtos.RentalContract.Respone;
using Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Repositories
{
    public interface IRentalContractRepository : IGenericRepository<RentalContract>
    {
        Task<IEnumerable<RentalContract>> GetByCustomerAsync(Guid customerId, int? status = null);

        Task<bool> HasActiveContractAsync(Guid customerId);

        Task<IEnumerable<RentalContract>> GetAllAsync(int? status = null, string? phone = null,
            string? citizenIdentity = null, string? driverLicense = null, Guid? checklistId = null);

        Task<RentalContract?> GetByChecklistIdAsync(Guid id);

        Task<IEnumerable<RentalContract>> GetByVehicleIdAsync(Guid vehicleId);

        Task<PageResult<RentalContract>> GetAllByPaginationAsync(int? status = null, string? phone = null, string? citizenIdentityNumber = null, string? driverLicenseNumber = null, Guid? stationId = null, PaginationParams? pagination = null);

        Task<PageResult<RentalContract>> GetMyContractsAsync(Guid customerId, PaginationParams pagination,
            int? status, Guid? stationId = null);

        Task<IEnumerable<RentalContract>> GetLateReturnContract();

        Task<IEnumerable<RentalContract>> GetExpiredContractAsync();

        Task<IEnumerable<RentalContract?>> GetAllRentalContractsAsync(Guid? stationId);

        Task<IEnumerable<BestRentedModel>> GetBestRentedModelsAsync(int months, int limit);
    }
}