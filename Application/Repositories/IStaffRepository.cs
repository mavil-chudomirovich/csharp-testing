using Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Repositories
{
    public interface IStaffRepository
    {
        Task<Guid> AddAsync(Staff staff);
        
        Task<Staff?> GetByUserIdAsync(Guid userId);

        Task<int> CountStaffsInStationAsync(Guid[] staffIds, Guid stationId);

        Task UpdateStationForDispatchAsync(Guid dispatchId, Guid stationId);
        Task<int> CountAvailableStaffInStationAsync(Guid stationId);

    }
}