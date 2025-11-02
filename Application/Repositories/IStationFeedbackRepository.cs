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
    public interface IStationFeedbackRepository : IGenericRepository<StationFeedback>
    {
        Task<PageResult<StationFeedback>> GetAllByPaginationAsync(PaginationParams pagination, Guid? stationId = null);
    }
}