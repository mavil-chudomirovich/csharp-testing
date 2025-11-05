using Application.Dtos.Station.Request;
using Application.Dtos.Station.Respone;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Abstractions
{
    public interface IStationService
    {
        Task<IEnumerable<StationViewRes>> GetAllStation();
        Task<StationViewRes?> GetByIdAsync(Guid id);
        Task<StationViewRes> CreateAsync(StationCreateReq request);
        Task<StationViewRes> UpdateAsync(StationUpdateReq request);
        Task DeleteAsync(Guid id);
    }
}
