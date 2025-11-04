using Application.Dtos.Common.Request;
using Application.Dtos.Common.Response;
using Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Repositories
{
    public interface IInvoiceRepository : IGenericRepository<Invoice>
    {
        Task<IEnumerable<Invoice>> GetByContractAsync(Guid ContractId);

        Task<Invoice?> GetByIdOptionAsync(Guid id, bool includeItems = false, bool includeDeposit = false);

        Task<PageResult<Invoice>> GetAllInvoicesAsync(PaginationParams pagination);
        Task<IEnumerable<Invoice>?> GetInvoiceByContractIdAndStatus(Guid? contractId, int? status);
    }
}