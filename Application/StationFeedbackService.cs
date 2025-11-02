using Application.Abstractions;
using Application.Constants;
using Application.Dtos.Common.Request;
using Application.Dtos.Common.Response;
using Application.Dtos.StationFeedback.Request;
using Application.Dtos.StationFeedback.Response;
using Application.Repositories;
using AutoMapper;
using Domain.Entities;
using static System.Collections.Specialized.BitVector32;

public class StationFeedbackService : IStationFeedbackService
{
    private readonly IStationFeedbackRepository _repo;
    private readonly IMapper _mapper;

    public StationFeedbackService(IStationFeedbackRepository repo, IMapper mapper)
    {
        _repo = repo;
        _mapper = mapper;
    }

    public async Task<StationFeedbackRes> CreateAsync(StationFeedbackCreateReq req, Guid customerId)
    {
        var feedback = _mapper.Map<StationFeedback>(req);
        feedback.Id = Guid.NewGuid();
        feedback.CustomerId = customerId;
        feedback.CreatedAt = DateTimeOffset.UtcNow;
        feedback.UpdatedAt = DateTimeOffset.UtcNow;
        await _repo.AddAsync(feedback);

        var created = await _repo.GetByIdAsync(feedback.Id);
        return _mapper.Map<StationFeedbackRes>(created);
    }

    public async Task DeleteAsync(Guid id)
    {
        var feedback = await _repo.GetByIdAsync(id);
        if (feedback == null)
            throw new Exception(Message.StationFeedbackMessage.NotFound);

        await _repo.DeleteAsync(id);
    }

    public async Task<IEnumerable<StationFeedbackRes>> GetByStationIdAsync(Guid stationId)
    {
        var list = await _repo.FindAsync(f => f.StationId == stationId, f => f.Customer) ?? [];
        return _mapper.Map<IEnumerable<StationFeedbackRes>>(list);
    }

    public async Task<IEnumerable<StationFeedbackRes>> GetByCustomerIdAsync(Guid customerId)
    {
        var list = await _repo.FindAsync(f => f.CustomerId == customerId, f => f.Customer) ?? [];
        return _mapper.Map<IEnumerable<StationFeedbackRes>>(list);
    }

    public async Task<PageResult<StationFeedbackRes>> GetAllAsync(PaginationParams pagination, Guid? stationId)
    {
        var page = await _repo.GetAllByPaginationAsync(pagination, stationId);
        var data = _mapper.Map<IEnumerable<StationFeedbackRes>>(page.Items ?? []);

        return new PageResult<StationFeedbackRes>(data, page.PageNumber, page.PageSize, page.Total);
    }

}