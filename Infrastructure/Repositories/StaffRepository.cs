using Application.Repositories;
using Domain.Entities;
using Infrastructure.ApplicationDbContext;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories
{
    public class StaffRepository : IStaffRepository
    {
        private readonly IGreenWheelDbContext _dbContext;

        public StaffRepository(IGreenWheelDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<Guid> AddAsync(Staff staff)
        {
            await _dbContext.Staffs.AddAsync(staff);
            await _dbContext.SaveChangesAsync();
            return staff.UserId;
        }

        public async Task<Staff?> GetByUserIdAsync(Guid userId)
        {
            return await _dbContext.Staffs
                .FirstOrDefaultAsync(s => s.UserId == userId && s.DeletedAt == null);
        }

        public async Task<int> CountStaffsInStationAsync(Guid[] staffIds, Guid stationId)
        {
            if (staffIds == null || staffIds.Length == 0)
                return 0;

            return await _dbContext.Staffs
                .CountAsync(s => staffIds.Contains(s.UserId)
                                 && s.StationId == stationId
                                 && s.DeletedAt == null);
        }

        public async Task UpdateStationForDispatchAsync(Guid dispatchId, Guid toStationId)
        {
            // Lấy danh sách staffIds từ bảng dispatch_request_staffs
            var staffIds = await _dbContext.DispatchRequestStaffs
                .Where(x => x.DispatchRequestId == dispatchId && x.DeletedAt == null)
                .Select(x => x.StaffId)
                .ToListAsync();

            if (staffIds == null || staffIds.Count == 0)
                return;

            // Lấy staff theo UserId (chính là StaffId)
            var staffs = await _dbContext.Staffs
                .Where(s => staffIds.Contains(s.UserId) && s.DeletedAt == null)
                .ToListAsync();

            if (staffs == null || staffs.Count == 0)
                return;

            // Cập nhật trạm mới
            foreach (var s in staffs)
            {
                s.StationId = toStationId;
            }

            await _dbContext.SaveChangesAsync();
        }

        public async Task<int> CountAvailableStaffInStationAsync(Guid stationId)
        {
            return await _dbContext.Staffs
                .Where(s => s.StationId == stationId && s.DeletedAt == null)
                .CountAsync();
        }
    }
}