using Application.Dtos.Common.Request;
using Application.Dtos.Common.Response;
using Application.Helpers;
using Application.Repositories;
using Domain.Entities;
using Infrastructure.ApplicationDbContext;
using Microsoft.EntityFrameworkCore;
namespace Infrastructure.Repositories
{
    public class StationFeedbackRepository : GenericRepository<StationFeedback>, IStationFeedbackRepository
    {
        public StationFeedbackRepository(IGreenWheelDbContext dbContext) : base(dbContext)
        {
        }

        public async Task<PageResult<StationFeedback>> GetAllByPaginationAsync(PaginationParams pagination, Guid? stationId = null)
        {
            var query = _dbContext.StationFeedbacks
                .Include(f => f.Customer)
                .OrderByDescending(f => f.CreatedAt)
                .AsQueryable();

            if (stationId != null)
                query = query.Where(f => f.StationId == stationId);

            var total = await query.CountAsync();
            var items = await query
                .ApplyPagination(pagination)
                .ToListAsync();

            return new PageResult<StationFeedback>(items, pagination.PageNumber, pagination.PageSize, total);
        }
    }
}