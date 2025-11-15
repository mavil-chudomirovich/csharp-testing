using Application.Constants;
using Application.Dtos.Common.Request;
using Application.Dtos.Common.Response;
using Application.Dtos.RentalContract.Respone;
using Application.Repositories;
using Domain.Entities;
using Infrastructure.ApplicationDbContext;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories
{
    public class RentalContractRepository : GenericRepository<RentalContract>, IRentalContractRepository
    {
        public RentalContractRepository(IGreenWheelDbContext dbContext) : base(dbContext)
        {
        }

        public async Task<IEnumerable<RentalContract>> GetByCustomerAsync(Guid customerId, int? status)
        {
            var contracts = _dbContext.RentalContracts.Where(r => r.CustomerId == customerId)
                .Include(x => x.Vehicle)
                    .ThenInclude(v => v == null ? null : v.Model)
                .Include(x => x.Station)
                 .Include(x => x.HandoverStaff)
                    .ThenInclude(h => h == null ? null : h.User)
                .Include(x => x.ReturnStaff)
                    .ThenInclude(h => h == null ? null : h.User)
                .OrderByDescending(x => x.CreatedAt)
                .AsQueryable();
            if (status != null)
            {
                contracts = contracts.Where(c => c.Status == status);
            }
            return await contracts.ToListAsync();
        }

        public async Task<IEnumerable<RentalContract>> GetAllAsync(int? status = null, string? phone = null,
            string? citizenIdentityNumber = null, string? driverLicenseNumber = null, Guid? stationId = null)
        {
            var rentalContracts = _dbContext.RentalContracts
                .Include(x => x.Vehicle)
                    .ThenInclude(v => v == null ? null : v.Model)
                .Include(x => x.Station)
                .Include(x => x.HandoverStaff)
                    .ThenInclude(h => h == null ? null : h.User)
                .Include(x => x.ReturnStaff)
                    .ThenInclude(h => h == null ? null : h.User)
                .Include(x => x.Customer)
                    .ThenInclude(u => u.CitizenIdentity)
                .Include(x => x.Customer)
                    .ThenInclude(u => u.DriverLicense)
                .OrderByDescending(x => x.CreatedAt)
                .AsQueryable();
            if (!string.IsNullOrEmpty(phone))
            {
                rentalContracts = rentalContracts.Where(rc => rc.Customer.Phone == phone);
            }
            if (status != null)
            {
                rentalContracts = rentalContracts.Where(rc => rc.Status == status);
            }
            if (!string.IsNullOrEmpty(citizenIdentityNumber))
            {
                rentalContracts = rentalContracts.Where(rc => rc.Customer.CitizenIdentity!.Number == citizenIdentityNumber);
            }
            if (!string.IsNullOrEmpty(driverLicenseNumber))
            {
                rentalContracts = rentalContracts.Where(rc => rc.Customer.DriverLicense!.Number == driverLicenseNumber);
            }
            if (stationId != null)
            {
                rentalContracts = rentalContracts.Where(rc => rc.StationId == stationId);
            }
            return await rentalContracts.ToListAsync();
        }

        public async Task<bool> HasActiveContractAsync(Guid customerId)
        {
            return await (_dbContext.RentalContracts.Where(r => r.CustomerId == customerId
            && r.Status != (int)RentalContractStatus.Completed
            && r.Status != (int)RentalContractStatus.Cancelled).FirstOrDefaultAsync()) != null;
        }

        public override async Task<RentalContract?> GetByIdAsync(Guid id)
        {
            return await _dbContext.RentalContracts.Where(r => r.Id == id)
                .Include(x => x.Vehicle)
                    .ThenInclude(v => v == null ? null : v.Model)
                .Include(x => x.Station)
                .Include(x => x.Invoices)
                    .ThenInclude(i => i.InvoiceItems)
                .Include(x => x.Invoices)
                    .ThenInclude(i => i.Deposit)
                .Include(x => x.VehicleChecklists)
                .Include(x => x.HandoverStaff)
                    .ThenInclude(h => h == null ? null : h.User)
                .Include(x => x.ReturnStaff)
                    .ThenInclude(h => h == null ? null : h.User)
                .Include(x => x.Customer)
                    .ThenInclude(u => u.CitizenIdentity)
                .Include(x => x.Customer)
                    .ThenInclude(u => u.DriverLicense)
                .FirstOrDefaultAsync();
        }

        public async Task<RentalContract?> GetByChecklistIdAsync(Guid id)
        {
            var vehicleChecklist = (await _dbContext.VehicleChecklists.Where(vc => vc.Id == id)
                .Include(vc => vc.Contract)
                    .ThenInclude(r => r == null ? null : r.Invoices).OrderBy(x => x.CreatedAt).FirstOrDefaultAsync());

            return vehicleChecklist == null ? null : vehicleChecklist.Contract;
        }

        public async Task<IEnumerable<RentalContract>> GetByVehicleIdAsync(Guid vehicleId)
        {
            var list = await _dbContext.RentalContracts.Where(c => c.VehicleId == vehicleId)
                    .Include(r => r.Customer)
                    .Include(r => r.Vehicle)
                        .ThenInclude(v => v == null ? null : v.Model)
                    .Include(r => r.Station)
                    .OrderByDescending(x => x.CreatedAt)
                    .ToListAsync();
            return list ?? [];
        }

        public async Task<PageResult<RentalContract>> GetAllByPaginationAsync(
            int? status = null,
            string? phone = null,
            string? citizenIdentityNumber = null,
            string? driverLicenseNumber = null,
            Guid? stationId = null,
            PaginationParams? pagination = null)
        {
            var query = _dbContext.RentalContracts
                .Include(x => x.Vehicle).ThenInclude(v => v == null ? null : v.Model)
                .Include(x => x.Station)
                .Include(x => x.HandoverStaff).ThenInclude(h => h == null ? null : h.User)
                .Include(x => x.ReturnStaff).ThenInclude(h => h == null ? null : h.User)
                .Include(x => x.Customer).ThenInclude(u => u.CitizenIdentity)
                .Include(x => x.Customer).ThenInclude(u => u.DriverLicense)
                .OrderByDescending(x => x.CreatedAt)
                .AsQueryable();

            if (!string.IsNullOrEmpty(phone))
                query = query.Where(rc => rc.Customer.Phone == phone);
            if (status != null)
                query = query.Where(rc => rc.Status == status);
            if (!string.IsNullOrEmpty(citizenIdentityNumber))
                query = query.Where(rc => rc.Customer.CitizenIdentity!.Number == citizenIdentityNumber);
            if (!string.IsNullOrEmpty(driverLicenseNumber))
                query = query.Where(rc => rc.Customer.DriverLicense!.Number == driverLicenseNumber);
            if (stationId != null)
                query = query.Where(rc => rc.StationId == stationId);

            var total = await query.CountAsync();

            if (pagination != null)
            {
                query = query
                    .Skip((pagination.PageNumber - 1) * pagination.PageSize)
                    .Take(pagination.PageSize);
            }

            var items = await query.ToListAsync();

            return new PageResult<RentalContract>(
                items,
                pagination?.PageNumber ?? 1,
                pagination?.PageSize ?? total,
                total
            );
        }

        public async Task<IEnumerable<RentalContract?>> GetAllRentalContractsAsync(Guid? stationId)
        {
            var query = _dbContext.RentalContracts
                .Include(x => x.Vehicle).ThenInclude(v => v == null ? null : v.Model)
                .Include(x => x.Station)
                .Include(x => x.HandoverStaff).ThenInclude(h => h == null ? null : h.User)
                .Include(x => x.ReturnStaff).ThenInclude(h => h == null ? null : h.User)
                .Include(x => x.Customer).ThenInclude(u => u.CitizenIdentity)
                .Include(x => x.Customer).ThenInclude(u => u.DriverLicense)
                .OrderByDescending(x => x.CreatedAt)
                .AsQueryable();

            if (stationId != null)
            {
                query = query.Where(rc => rc.StationId == stationId);
            }

            return await query.ToListAsync();
        }

        public async Task<PageResult<RentalContract>> GetMyContractsAsync(
            Guid customerId, PaginationParams pagination,
            int? status, Guid? stationId = null)
        {
            var query = _dbContext.RentalContracts
                .Include(rc => rc.Vehicle).ThenInclude(v => v == null ? null : v.Model)
                .Include(rc => rc.Station)
                .Include(rc => rc.HandoverStaff).ThenInclude(s => s == null ? null : s.User)
                .Include(rc => rc.ReturnStaff).ThenInclude(s => s == null ? null : s.User)
                .OrderByDescending(x => x.CreatedAt)
                .Where(rc => rc.CustomerId == customerId);

            if (status != null)
                query = query.Where(rc => rc.Status == status);
            if (stationId != null)
                query = query.Where(rc => rc.StationId == stationId);

            var total = await query.CountAsync();

            var items = await query
                .OrderByDescending(rc => rc.CreatedAt)
                .Skip((pagination.PageNumber - 1) * pagination.PageSize)
                .Take(pagination.PageSize)
                .ToListAsync();

            return new PageResult<RentalContract>(
                items,
                pagination.PageNumber,
                pagination.PageSize,
                total
            );
        }

        public async Task<IEnumerable<RentalContract>> GetLateReturnContract()
        {
            return await _dbContext.RentalContracts.Where(r => r.Status == (int)RentalContractStatus.Active
                                                            && r.ActualEndDate == null
                                                            && r.ActualStartDate != null
                                                            && r.EndDate < DateTimeOffset.UtcNow)
                                                    .Include(r => r.Customer)
                                                    .Include(r => r.Vehicle)
                                                        .ThenInclude(v => v!.Model)
                                                    .Include(r => r.Station)
                                                    .ToArrayAsync();
        }

        public async Task<IEnumerable<RentalContract>> GetExpiredContractAsync()
        {
            return await _dbContext.RentalContracts.Where(r => (r.Status == (int)RentalContractStatus.Active || r.Status == (int)RentalContractStatus.PaymentPending || r.Status == (int)RentalContractStatus.RequestPeding)
                                                            && r.ActualStartDate == null
                                                            && r.EndDate < DateTimeOffset.UtcNow)
                                                    .Include(r => r.Customer)
                                                    .Include(r => r.Vehicle)
                                                        .ThenInclude(v => v!.Model)
                                                    .Include(r => r.Station)
                                                    .ToArrayAsync();
        }

        public async Task<IEnumerable<BestRentedModel>> GetBestRentedModelsAsync(int months, int limit)
        {
            var fromDate = DateTimeOffset.UtcNow.AddMonths(-months);

            var query =
                from vm in _dbContext.VehicleModels
                join v in _dbContext.Vehicles on vm.Id equals v.ModelId
                join rc in _dbContext.RentalContracts on v.Id equals rc.VehicleId
                where rc.StartDate >= fromDate
                      && rc.DeletedAt == null
                group rc by new { vm.Id, vm.Name } into g
                orderby g.Count() descending
                select new BestRentedModel
                {
                    ModelId = g.Key.Id,
                    ModelName = g.Key.Name,
                    RentedCount = g.Count()
                };

            return await query.Take(limit).ToListAsync();
        }
    }
}