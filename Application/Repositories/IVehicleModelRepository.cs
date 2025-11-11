using Domain.Entities;

namespace Application.Repositories
{
    public interface IVehicleModelRepository : IGenericRepository<VehicleModel>
    {
        Task<IEnumerable<VehicleModel>> FilterVehicleModelsAsync(Guid stationId,
                                                            DateTimeOffset startDate,
                                                            DateTimeOffset endDate,
                                                            Guid? segmentId = null);
        Task<VehicleModel?> GetByIdAsync(Guid id, Guid stationId, DateTimeOffset startDate,
        DateTimeOffset endDate);
        Task<IEnumerable<VehicleModel>> GetAllAsync(string? name, Guid? segmentId);
        Task<VehicleModel?> GetWithoutSearchAsync(Guid id);
    }
}
