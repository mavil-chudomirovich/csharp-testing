using Application.Dtos.Common.Request;
using Application.Dtos.Common.Response;
using Application.Dtos.StationFeedback.Request;
using Application.Dtos.StationFeedback.Response;

namespace Application.Abstractions
{
    public interface IStationFeedbackService
    {
        Task<StationFeedbackRes> CreateAsync(StationFeedbackCreateReq req, Guid customerId);

        Task<IEnumerable<StationFeedbackRes>> GetByStationIdAsync(Guid stationId);

        Task<IEnumerable<StationFeedbackRes>> GetByCustomerIdAsync(Guid customerId);

        Task DeleteAsync(Guid id);
        Task<PageResult<StationFeedbackRes>> GetAllAsync(PaginationParams pagination, Guid? stationId);
    }
}