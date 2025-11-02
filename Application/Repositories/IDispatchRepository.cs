using Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace Application.Repositories
{
    public interface IDispatchRepository : IGenericRepository<DispatchRequest>
    {
        Task<DispatchRequest?> GetByIdWithFullInfoAsync(Guid id);

        Task<IEnumerable<DispatchRequest>> GetAllExpandedAsync(
            Guid? fromStationId,
            Guid? toStationId,
            int? status
        );
        Task ClearDispatchRelationsAsync(Guid dispatchId);
        Task AddDispatchRelationsAsync(
            IEnumerable<DispatchRequestStaff> staffs,
            IEnumerable<DispatchRequestVehicle> vehicles);
    }
}