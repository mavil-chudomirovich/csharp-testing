using Application.Constants;
using Application.Dtos.Common.Request;
using Application.Dtos.Common.Response;
using Application.Helpers;
using Application.Repositories;
using Domain.Entities;
using Infrastructure.ApplicationDbContext;
using Microsoft.EntityFrameworkCore;
using System.Linq.Dynamic.Core;

namespace Infrastructure.Repositories
{
    public class InvoiceRepository : GenericRepository<Invoice>, IInvoiceRepository
    {
        public InvoiceRepository(IGreenWheelDbContext dbContext) : base(dbContext)
        {
        }

        public async Task<IEnumerable<Invoice>?> GetInvoiceByContractIdAndStatus(Guid? contractId, int? status)
        {
            var invoices = await _dbContext.Invoices
                .Include(i => i.InvoiceItems)
                .Include(i => i.Contract).OrderByDescending(i => i.CreatedAt).ToListAsync();
            if (contractId != null)
            {
                invoices = (List<Invoice>)invoices.Where(i => i.Contract.Id == contractId);
            }
            if (status != null)
            {
                invoices = (List<Invoice>)invoices.Where(i => i.Status == status);
            }
            return invoices;
        }

        public async Task<IEnumerable<Invoice>> GetByContractAsync(Guid ContractId)
        {
            return await _dbContext.Invoices.Where(i => i.ContractId == ContractId).ToListAsync();
        }

        public async Task<Invoice?> GetByIdOptionAsync(Guid id, bool includeItems = false, bool includeDeposit = false)
        {
            IQueryable<Invoice> query = _dbContext.Invoices
                                        .Include(i => i.Contract)
                                            .ThenInclude(r => r.Customer)
                                         .OrderByDescending(i => i.CreatedAt)
                                        .AsQueryable();
            if (includeItems)
            {
                query = query.Include(i => i.InvoiceItems)
                    .ThenInclude(i => i.ChecklistItem)
                        .ThenInclude(cli => cli == null ? null : cli.Component);
            }
            if (includeDeposit)
            {
                query = query.Include(i => i.Deposit);
            }
            return await query.FirstOrDefaultAsync(i => i.Id == id);
        }

        public async Task<PageResult<Invoice>> GetAllWithPaginationAsync(PaginationParams pagination)
        {
            var query = _dbContext.Invoices
                .Include(i => i.Contract)
                .Include(i => i.InvoiceItems)
                .AsNoTracking()
                .OrderByDescending(i => i.CreatedAt);

            var totalCount = await query.CountAsync();

            var items = await query
                .ApplyPagination(pagination)
                .ToListAsync();

            return new PageResult<Invoice>(items, pagination.PageNumber, pagination.PageSize, totalCount);
        }

        public async Task<IEnumerable<Invoice>> GetRefundInvoiceWarningAsync()
        {
            return await _dbContext.Invoices.Where(r => r.Type == (int)InvoiceType.Refund
                                                && r.Status == (int)InvoiceStatus.Pending
                                                && InvoiceHelper.CalculateTotalAmount(r) > 0
                                                && (DateTimeOffset.UtcNow - r.CreatedAt).TotalDays >= 7)
                                                    .Include(r => r.Contract)
                                                        .ThenInclude(c => c.Customer)
                                                    .Include(r => r.Contract)
                                                        .ThenInclude(c => c.Station)
                                                    .ToArrayAsync();
        }

        public async Task<IEnumerable<Invoice>> GetAllInvoicesAsync(Guid? stationId)
        {
            var query = _dbContext.Invoices
                .Include(i => i.Contract)
                .Include(i => i.InvoiceItems)
                .AsNoTracking()
                .OrderByDescending(i => i.CreatedAt)
                .AsQueryable();

            if (stationId != null)
            {
                query = query.Where(i => i.Contract.StationId == stationId);
            }

            return await query.ToListAsync();
        }
    }
}